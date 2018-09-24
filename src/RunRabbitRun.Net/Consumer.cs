using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RunRabbitRun.Net.DryIoc;
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
        private List<ConsumeHandler> consumeHandlers = new List<ConsumeHandler>();

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
            var consumerTypeAttributes = GetTypeAttributes(consumerType);
            var channelAttribute = FindAttribute<ChannelAttribute>(consumerTypeAttributes);
            if (channelAttribute != null)
            {
                (IModel channel, string channelName) = EnsureChannelModel(channelAttribute);
                consumerMainChannel = channel;

                var qosAttribute = FindAttribute<QosAttribute>(consumerTypeAttributes);
                if (qosAttribute != null)
                    consumerMainChannel.BasicQos(0, qosAttribute.PrefetchCount, false);
            }
            else
            {
                (IModel channel, string channelName) = EnsureChannelModel(new ChannelAttribute("RunRabbitRunDefaultChannel"));
                consumerMainChannel = channel;
            }

            EnsureExchangesExists(consumerMainChannel, consumerTypeAttributes);
            EnsureExchangeToExchangeBinding(consumerMainChannel, consumerTypeAttributes);

            BuildConsumeHandlers();
        }

        public IEnumerable<Attribute> GetTypeAttributes(Type type)
        {
            List<Attribute> attributes = new List<Attribute>();
            Type getFromType = type;
            while (getFromType != null)
            {
                attributes.AddRange(getFromType.GetTypeInfo().GetAttributes(null, true));
                var interfaces = getFromType.GetInterfaces();
                foreach (var interf in interfaces)
                {
                    attributes.AddRange(GetTypeAttributes(interf));
                }
                getFromType = getFromType.GetBaseType();
            }
            return attributes;
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

                EnsureQueueExists(consumeMethodChannel, consumeMethodAttributes);

                var consumeAttribute = FindAttribute<ConsumeAttribute>(consumeMethodAttributes);

                var consumeQueue = consumeAttribute.Queue;

                if (string.IsNullOrEmpty(consumeQueue))
                {
                    var queueAttributes = FindManyAttributes<QueueAttribute>(consumeMethodAttributes);
                    var queueAttribute = queueAttributes.First();
                    consumeQueue = queueAttribute.Queue;
                }

                var queueNameBuilder = publicDependenciesContainer
                    .Resolve<IQueueNameBuilder>(IfUnresolved.ReturnDefault);
                if (queueNameBuilder != null)
                {
                    consumeQueue = queueNameBuilder.Build(consumeQueue);
                }

                var consumeHandler = new ConsumeHandler(
                    consumeInstance,
                    consumeHandlerMethod,
                    consumeMethodChannel,
                    consumeQueue,
                    consumeAttribute.AutoAck,
                    publicDependenciesContainer.CreateFacade(),
                    GetBeforeMethod(),
                    GetAfterMethod(),
                    GetErrorMethod()
                );
                consumeHandlers.Add(consumeHandler);
            }
        }

        private MethodInfo GetBeforeMethod()
        {
            return consumerType
                .GetTypeInfo()
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(method => method.Name == "OnBefore");
        }

        private MethodInfo GetAfterMethod()
        {
            return consumerType
                .GetTypeInfo()
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(method => method.Name == "OnAfter");
        }

        public MethodInfo GetErrorMethod()
        {
            return consumerType
                .GetTypeInfo()
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(method => method.Name == "OnError");
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

        private void EnsureExchangesExists(IModel consumeMethodChannel, IEnumerable<Attribute> attributes)
        {
            var exchangeAttributes = FindManyAttributes<ExchangeAttribute>(attributes);

            if (exchangeAttributes.Count() == 0)
                return;

            var rabbitMqConnection = internalDependenciesContainer.Resolve<IConnection>();

            foreach (var exchangeAttribute in exchangeAttributes)
            {
                Dictionary<string, object> arguments = new Dictionary<string, object>();
                if (!string.IsNullOrEmpty(exchangeAttribute.AlternateExchange))
                    arguments.Add("alternate-exchange", exchangeAttribute.AlternateExchange);

                consumeMethodChannel
                    .ExchangeDeclare(
                        exchangeAttribute.Name,
                        exchangeAttribute.Type,
                        exchangeAttribute.Durable,
                        exchangeAttribute.AutoDelete,
                        arguments);
            }
        }

        private void EnsureExchangeToExchangeBinding(IModel consumeMethodChannel, IEnumerable<Attribute> attributes)
        {
            var exchangeToExchangeBindingAttributes = FindManyAttributes<ExchangeToExchangeBindingAttribute>(attributes);

            if (exchangeToExchangeBindingAttributes.Count() == 0)
                return;

            var rabbitMqConnection = internalDependenciesContainer.Resolve<IConnection>();

            foreach (var bindingAttribute in exchangeToExchangeBindingAttributes)
            {
                consumeMethodChannel.ExchangeBind(bindingAttribute.Destination, bindingAttribute.Source, bindingAttribute.RoutingKey, null);
            }
        }

        private void EnsureQueueExists(IModel consumeMethodChannel, IEnumerable<Attribute> attributes)
        {
            var queueAttributes = FindManyAttributes<QueueAttribute>(attributes);

            if (queueAttributes.Count() == 0)
                return;

            var rabbitMqConnection = internalDependenciesContainer.Resolve<IConnection>();
            var queueNameBuilder = publicDependenciesContainer
                    .Resolve<IQueueNameBuilder>(IfUnresolved.ReturnDefault);

            foreach (var queueAttribute in queueAttributes)
            {
                var queue = queueAttribute.Queue;
                if (queueNameBuilder != null)
                {
                    queue = queueNameBuilder.Build(queue);
                }
                consumeMethodChannel.QueueDeclare(queue: queue,
                         durable: queueAttribute.Durable,
                         exclusive: queueAttribute.Exclusive,
                         autoDelete: queueAttribute.AutoDelete,
                         arguments: null);

                foreach (var route in queueAttribute.RoutingKey)
                {
                    consumeMethodChannel.QueueBind(
                        queue,
                        queueAttribute.Exchange,
                        route,
                        null);
                }
            }
        }
    }
}