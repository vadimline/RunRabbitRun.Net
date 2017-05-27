using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DryIoc;
using RunRabbitRun.Net.Attributes;
using RabbitMQ.Client;

namespace RunRabbitRun.Net
{
    internal class Consumer
    {
        private Type consumerType;
        private IContainer publicDependenciesContainer;
        private IContainer internalDependenciesContainer;

        private IContainer channelsContainer;

        private List<string> channelsKeys = new List<string>();
        private IModel consumerMainChannel = null;

        public Consumer(
            Type consumerType,
            IContainer publicDependenciesContainer,
            IContainer internalDependenciesContainer,
            IContainer channelsContainer)
        {
            this.consumerType = consumerType;
            this.channelsContainer = channelsContainer;
            this.internalDependenciesContainer = internalDependenciesContainer;
            this.publicDependenciesContainer = publicDependenciesContainer;

            if (!consumerType.IsInterface())
                this.internalDependenciesContainer.Register(consumerType);

            this.Init();
        }

        private void Init()
        {
            var consumerTypeAttributes = consumerType.GetTypeInfo().GetAttributes(null, true);
            var channelAttribute = FindAttribute<ChannelAttribute>(consumerTypeAttributes);
            if (channelAttribute != null)
            {
                (IModel channel, string channelName) = EnsureChannelModel(channelAttribute);
                consumerMainChannel = channel;

                var qosAttribute = FindAttribute<QosAttribute>(consumerTypeAttributes);
                if (qosAttribute != null)
                    consumerMainChannel.BasicQos(0, qosAttribute.PrefetchCount, false);
            }

            EnsureExchangesExists(consumerTypeAttributes);
            EnsureExchangeToExchangeBinding(consumerTypeAttributes);

            BuildConsumeHandlers();
        }

        private void BuildConsumeHandlers()
        {
            var consumeHandlerMethods = consumerType
                                            .GetTypeInfo()
                                            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                                            .Where(method => method.GetCustomAttribute<ConsumeAttribute>() != null);

            var consumeInstance = internalDependenciesContainer.Resolve(consumerType);

            foreach (var consumeHandlerMethod in consumeHandlerMethods)
            {
                var consumeMethodAttributes = consumeHandlerMethod.GetAttributes();

                IModel consumeMethodChannel = SetupConsumeMethodChannel(consumeMethodAttributes) ?? consumerMainChannel;

                EnsureQueueExists(consumeMethodAttributes);

                var consumeAttribute = FindAttribute<ConsumeAttribute>(consumeMethodAttributes);

                var consumeQueue = consumeAttribute.Queue;

                if (string.IsNullOrEmpty(consumeQueue))
                {
                    var queueAttributes = FindManyAttributes<QueueAttribute>(consumeMethodAttributes);
                    consumeQueue = queueAttributes.First().Queue;
                }

                var consumeHandler = new ConsumeHandler(
                    consumeInstance,
                    consumeHandlerMethod,
                    consumeMethodChannel,
                    consumeQueue,
                    consumeAttribute.AutoAck,
                    publicDependenciesContainer.CreateFacade()
                );
            }
        }

        private IModel SetupConsumeMethodChannel(IEnumerable<Attribute> attributes)
        {
            var channelAttribute = FindAttribute<ChannelAttribute>(attributes);
            if (channelAttribute != null)
            {
                (IModel channel, string channelName) = EnsureChannelModel(channelAttribute);

                var qosAttribute = FindAttribute<QosAttribute>(attributes);
                if (qosAttribute != null)
                    channel.BasicQos(0, qosAttribute.PrefetchCount, false);
                return channel;
            }
            else
            {
                if (consumerMainChannel == null)
                {
                    var generatedChannelName = Guid.NewGuid().ToString();
                    return CreateChannelModel(generatedChannelName);
                }
            }

            return null;
        }

        private T FindAttribute<T>(IEnumerable<Attribute> attributes) where T : Attribute
        {
            return attributes.FirstOrDefault(attribute => attribute is T) as T;
        }

        private IEnumerable<T> FindManyAttributes<T>(IEnumerable<Attribute> attributes) where T : Attribute
        {
            return attributes.OfType<T>();
        }

        private (IModel channel, string channelName) EnsureChannelModel(ChannelAttribute channelAttribute)
        {
            if (!string.IsNullOrEmpty(channelAttribute.Name))
            {
                if (channelsContainer.IsRegistered<IModel>(serviceKey: channelAttribute.Name))
                    return (channelsContainer.Resolve<IModel>(serviceKey: channelAttribute.Name), channelAttribute.Name);
                else
                    return (CreateChannelModel(channelAttribute.Name), channelAttribute.Name);
            }

            var generatedChannelName = Guid.NewGuid().ToString();
            return (CreateChannelModel(generatedChannelName), generatedChannelName);
        }

        private IModel CreateChannelModel(string name)
        {
            var rabbitMqConnection = internalDependenciesContainer.Resolve<IConnection>();
            var channelModel = rabbitMqConnection.CreateModel();

            channelsKeys.Add(name);
            channelsContainer.UseInstance(channelModel, true, false, name);

            return channelModel;
        }

        private void EnsureExchangesExists(IEnumerable<Attribute> attributes)
        {
            var exchangeAttributes = FindManyAttributes<ExchangeAttribute>(attributes);

            if (exchangeAttributes.Count() == 0)
                return;

            var rabbitMqConnection = internalDependenciesContainer.Resolve<IConnection>();

            using (IModel declareModel = rabbitMqConnection.CreateModel())
            {
                foreach (var exchangeAttribute in exchangeAttributes)
                {
                    Dictionary<string, object> arguments = new Dictionary<string, object>();
                    if (!string.IsNullOrEmpty(exchangeAttribute.AlternateExchange))
                        arguments.Add("alternate-exchange", exchangeAttribute.AlternateExchange);

                    declareModel
                        .ExchangeDeclare(
                            exchangeAttribute.Name,
                            exchangeAttribute.Type,
                            exchangeAttribute.Durable,
                            exchangeAttribute.AutoDelete,
                            arguments);
                }
            }
        }

        private void EnsureExchangeToExchangeBinding(IEnumerable<Attribute> attributes)
        {
            var exchangeToExchangeBindingAttributes = FindManyAttributes<ExchangeToExchangeBindingAttribute>(attributes);

            if (exchangeToExchangeBindingAttributes.Count() == 0)
                return;

            var rabbitMqConnection = internalDependenciesContainer.Resolve<IConnection>();

            using (IModel declareModel = rabbitMqConnection.CreateModel())
            {
                foreach (var bindingAttribute in exchangeToExchangeBindingAttributes)
                {
                    declareModel.ExchangeBind(bindingAttribute.Destination, bindingAttribute.Source, bindingAttribute.RoutingKey, null);
                }
            }
        }

        private void EnsureQueueExists(IEnumerable<Attribute> attributes)
        {
            var queueAttributes = FindManyAttributes<QueueAttribute>(attributes);

            if (queueAttributes.Count() == 0)
                return;

            var rabbitMqConnection = internalDependenciesContainer.Resolve<IConnection>();

            using (IModel declareModel = rabbitMqConnection.CreateModel())
            {
                foreach (var queueAttribute in queueAttributes)
                {
                    declareModel.QueueDeclare(queue: queueAttribute.Queue,
                             durable: queueAttribute.Durable,
                             exclusive: queueAttribute.Exclusive,
                             autoDelete: queueAttribute.AutoDelete,
                             arguments: null);

                    declareModel.QueueBind(
                        queueAttribute.Queue,
                        queueAttribute.Exchange,
                        queueAttribute.RoutingKey,
                        null);
                }
            }
        }
    }
}