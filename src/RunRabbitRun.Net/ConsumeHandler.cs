using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using RunRabbitRun.Net.DryIoc;
using RunRabbitRun.Net.Attributes;
using RunRabbitRun.Net.Resolvers;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace RunRabbitRun.Net
{
    public class ConsumeHandler
    {
        private object consumerInstance;
        private MethodInfo consumeMethod;
        private IModel channelModel;
        private string consumeQueueName;
        private bool autoAck;
        private IContainer dependenciesContainer;
        private IRabbitEventingBasicConsumer eventingBasicConsumer;
        private bool isReturnResponse;
        private List<ParameterResolver> parameterResolvers = new List<ParameterResolver>();
        private MethodInfo beforeMethod;
        private MethodInfo afterMethod;
        private MethodInfo errorMethod;

        public ConsumeHandler(
            object consumerInstance,
            MethodInfo consumeMethod,
            IModel channelModel,
            string consumeQueueName,
            bool autoAck,
            IContainer dependenciesContainer,
            MethodInfo beforeMethod,
            MethodInfo afterMethod,
            MethodInfo errorMethod)
        {
            this.consumerInstance = consumerInstance;
            this.consumeMethod = consumeMethod;
            this.channelModel = channelModel;
            this.consumeQueueName = consumeQueueName;
            this.autoAck = autoAck;
            this.dependenciesContainer = dependenciesContainer;
            this.beforeMethod = beforeMethod;
            this.afterMethod = afterMethod;
            this.errorMethod = errorMethod;

            this.eventingBasicConsumer = GetConsumer();
            this.eventingBasicConsumer.Received += OnMessageReceived;

            isReturnResponse = ShouldExpectResponse();
            this.ReadMethodArguments();
            this.Consume();
        }

        private IRabbitEventingBasicConsumer GetConsumer()
        {
            var eventintBasicConsumerInstance = dependenciesContainer
                .Resolve<IRabbitEventingBasicConsumer>(IfUnresolved.ReturnDefault);
            if (eventintBasicConsumerInstance != null)
                return eventintBasicConsumerInstance;

            return new RabbitEventingBasicConsumer(this.channelModel);
        }

        private bool ShouldExpectResponse()
        {
            var returnType = this.consumeMethod.ReturnType;

            return returnType.IsGeneric();
        }
        private void ReadMethodArguments()
        {
            //TODO: Check in MSDN if GetParameters returns parameters in right order, then OrderBy is redundant
            var parameters = this.consumeMethod.GetParameters().OrderBy(param => param.Position);

            foreach (var parameter in parameters)
            {
                var parameterAttribute = parameter.GetCustomAttribute(typeof(ParameterAttribute), true) as ParameterAttribute;
                if (parameterAttribute != null)
                {
                    parameterResolvers.Add(parameterAttribute.GetParameterResolver(parameter));
                }
            }
        }

        private async void OnMessageReceived(object sender, BasicDeliverEventArgs args)
        {
            await InvokeOnBefore(consumeQueueName, args);
            using (var scope = dependenciesContainer.OpenScope())
            {
                Action ack = null;
                Action<bool> reject = null;
                bool ackOrRejectInvoked = false;
                if (!autoAck)
                {
                    ack = () =>
                    {
                        if (!ackOrRejectInvoked)
                        {
                            ackOrRejectInvoked = true;
                            channelModel.BasicAck(args.DeliveryTag, false);
                        }
                    };
                    reject = (bool requeue) =>
                    {
                        if (!ackOrRejectInvoked)
                        {
                            ackOrRejectInvoked = true;
                            channelModel.BasicReject(args.DeliveryTag, requeue);
                        }
                    };
                }

                try
                {
                    List<object> arguments = new List<object>();
                    foreach (var parameterResolver in parameterResolvers)
                    {
                        arguments.Add(parameterResolver.Resolve(scope, args));
                    }

                    if (!autoAck)
                    {
                        arguments.Add(ack);
                        arguments.Add(reject);
                    }

                    if (isReturnResponse)
                        await InvokeAndProcessResponseAsync(args, arguments.ToArray()).ConfigureAwait(false);
                    else
                        await InvokeAndForget(arguments.ToArray()).ConfigureAwait(false);


                }
                catch (Exception ex)
                {
                    await InvokeOnError(consumeQueueName, args, ex);
                    if (!autoAck)
                    {
                        reject(false);
                    }
                }
            }
        }

        public Task InvokeOnBefore(params object[] arguments)
        {
            try
            {
                if (beforeMethod != null)
                    return (Task)beforeMethod.Invoke(consumerInstance, arguments);

                return Task.FromResult(-1);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return Task.FromResult(-1);
            }
        }

        public Task InvokeOnAfter(params object[] arguments)
        {
            try
            {
                if (afterMethod != null)
                    return (Task)afterMethod.Invoke(consumerInstance, arguments);

                return Task.FromResult(-1);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return Task.FromResult(-1);
            }
        }

        public Task InvokeOnError(params object[] arguments)
        {
            try
            {
                if (errorMethod != null)
                    return (Task)errorMethod.Invoke(consumerInstance, arguments);

                return Task.FromResult(-1);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return Task.FromResult(-1);
            }
        }

        private Task InvokeAndForget(params object[] arguments)
        {
            return (Task)consumeMethod.Invoke(consumerInstance, arguments);
        }

        private async Task InvokeAndProcessResponseAsync(BasicDeliverEventArgs args, params object[] arguments)
        {
            var response = await ((Task<Response>)consumeMethod.Invoke(consumerInstance, arguments)).ConfigureAwait(false);
            if (response == null)
            {
                //TODO: What should we do in this case???
                return;
            }

            var basicProperties = channelModel.CreateBasicProperties();
            basicProperties.ContentEncoding = response.ContentEncoding;

            if (args.BasicProperties.IsCorrelationIdPresent())
                basicProperties.CorrelationId = args.BasicProperties.CorrelationId;

            if (response.Headers.Count > 0)
                basicProperties.Headers = response.Headers;
            else
                basicProperties.Headers = new Dictionary<string, object>();

            if (!args.BasicProperties.IsHeadersPresent())
                return;

            if (!args.BasicProperties.Headers.ContainsKey("rabbit-callback-id"))
                return;

            var callBackId = Encoding.UTF8.GetString((byte[])args.BasicProperties.Headers["rabbit-callback-id"]) as string;

            basicProperties.Headers.Add("rabbit-callback-id", callBackId);

            channelModel.BasicPublish(
                response.ReplyExchange, //TADADADAM : what exchange to use? TODO: Implement default reply exchange for Rabbit
                response.ReplyTo ?? args.BasicProperties.ReplyTo,
                false,
                basicProperties,
                SerializeBody(response.ContentEncoding, response.Body)
            );
        }

        private byte[] SerializeBody(string contentEncoding, string body)
        {
            if (string.IsNullOrEmpty(body))
                return null;

            var encoding = System.Text.Encoding.GetEncoding(contentEncoding);
            return encoding.GetBytes(body);
        }

        private void Consume()
        {
            channelModel.BasicConsume(consumeQueueName,
                autoAck,
                (IBasicConsumer)eventingBasicConsumer);
        }
    }
}