using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DryIoc;
using RunRabbitRun.Net.Attributes;
using RunRabbitRun.Net.Parameters;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Newtonsoft.Json;

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
        private EventingBasicConsumer eventBasicConsumer;

        private bool isReturnResponse;
        private List<ParameterResolver> parameterResolvers = new List<ParameterResolver>();

        public ConsumeHandler(
            object consumerInstance,
            MethodInfo consumeMethod,
            IModel channelModel,
            string consumeQueueName,
            bool autoAck,
            IContainer dependenciesContainer)
        {
            this.consumerInstance = consumerInstance;
            this.consumeMethod = consumeMethod;
            this.channelModel = channelModel;
            this.consumeQueueName = consumeQueueName;
            this.autoAck = autoAck;
            this.dependenciesContainer = dependenciesContainer;

            this.dependenciesContainer.UseInstance<IModel>(channelModel);
            this.eventBasicConsumer = GetConsumer();
            this.eventBasicConsumer.Received += OnMessageReceived;

            isReturnResponse = ShouldExpectResponse();
            this.ReadMethodArguments();
            this.Consume();
        }

        private EventingBasicConsumer GetConsumer(){
            if(dependenciesContainer.IsRegistered(typeof(EventingBasicConsumer)))
                return dependenciesContainer.Resolve<EventingBasicConsumer>();

            return new EventingBasicConsumer(this.channelModel);
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

        private void OnMessageReceived(object sender, BasicDeliverEventArgs args)
        {
            Task.Run(async () =>
            {
                Action ack = null;
                Action<bool> reject = null;
                if (!autoAck)
                {
                    ack = () => channelModel.BasicAck(args.DeliveryTag, false);
                    reject = (bool requeue) => channelModel.BasicReject(args.DeliveryTag, requeue);
                }

                try
                {
                    List<object> arguments = new List<object>();
                    foreach (var parameterResolver in parameterResolvers)
                    {
                        arguments.Add(parameterResolver.Resolve(dependenciesContainer, args));
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
                    System.Diagnostics.Debug.Write(ex);
                    if (!autoAck)
                    {
                        reject(false);
                    }
                }
            });
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

            channelModel.BasicPublish(
                response.ReplyExchange, //TADADADAM : what exchange to use?
                response.ReplyTo ?? args.BasicProperties.ReplyTo,
                false,
                basicProperties,
                SerializeBody(response.ContentEncoding, response.Body)
            );
        }

        private byte[] SerializeBody(string contentEncoding, string body)
        {
            var encoding = System.Text.Encoding.GetEncoding(contentEncoding);
            return encoding.GetBytes(body);
        }

        private void Consume()
        {
            channelModel.BasicConsume(consumeQueueName,
                autoAck,
                eventBasicConsumer);
        }
    }
}