using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RunRabbitRun.Net
{
    public class RabbitHole : IDisposable
    {
        private IConnection connection;
        private string exchange;
        private IModel channel;
        private string rabbitHoleId;
        private string rabbitHoleQueueName;
        private EventingBasicConsumer consumer;
        private Dictionary<string, Action<Response>> cunsumersCallbacks = new Dictionary<string, Action<Response>>();
        private object consumersLock = new object();
        public RabbitHole(string replyExchange, IConnection connection)
        {
            this.connection = connection;
            this.exchange = replyExchange;
            rabbitHoleId = Guid.NewGuid().ToString();
            rabbitHoleQueueName = $"rh-{rabbitHoleId}";
            channel = connection.CreateModel();
            InitQueue();
        }
        private void InitQueue()
        {
            channel.QueueDeclare(rabbitHoleQueueName,
            false, true, true, null);
            channel.QueueBind(rabbitHoleQueueName, exchange, $"");
            consumer = new EventingBasicConsumer(channel);
            consumer.Received += OnMessage;
            channel.BasicConsume(rabbitHoleQueueName, true, consumer);
        }

        private void OnMessage(object sender, BasicDeliverEventArgs args)
        {
            if (!args.BasicProperties.IsCorrelationIdPresent())
                return;

            if (!cunsumersCallbacks.ContainsKey(args.BasicProperties.CorrelationId))
                return;

            Action<Response> consumer = null;

            cunsumersCallbacks.TryGetValue(args.BasicProperties.CorrelationId, out consumer);
            if (consumer == null)
                return;

            Response response = new Response();
            response.Headers = args.BasicProperties.Headers as Dictionary<string, object>;
            response.ReplyTo = args.BasicProperties.ReplyTo;
            response.ContentEncoding = args.BasicProperties.ContentEncoding;
            response.Body = GetBody(args);

            consumer(response);
        }

        //TODO: Same logic is used in multiple places, consider move it to class
        private string GetBody(BasicDeliverEventArgs args)
        {
            var encoding = System.Text.Encoding.GetEncoding(args.BasicProperties.ContentEncoding);
            return encoding.GetString(args.Body);
        }

        //TODO: Same logic is used in multiple places, consider move it to class
        private byte[] GetBody(string contentEncoding, string body)
        {
            var encoding = System.Text.Encoding.GetEncoding(contentEncoding);
            return encoding.GetBytes(body);
        }

        public async Task<Response> SendAsync(Request request)
        {
            Response response = null;
            string correlationId = Guid.NewGuid().ToString();

            var delayCancelToken = new CancellationTokenSource();

            void ResponseRecieved(Response r)
            {
                response = r;
                delayCancelToken.Cancel();
            }
            RegisterConsumer(correlationId, ResponseRecieved);

            Send(correlationId, request);

            await Task.Delay(10000, delayCancelToken.Token).ContinueWith(task =>
            {
                UnregisterConsumer(correlationId);
                if (!task.IsCanceled)
                        // This mean that delay task complete and no response recieved
                        throw new TimeoutException("No response recieved");
            });

            return response;
        }

        private void Send(string correlationId, Request request)
        {
            var properties = channel.CreateBasicProperties();
            properties.ReplyTo = request.ReplyRoutingKeyPrefix + $".{rabbitHoleId}";
            properties.ContentEncoding = request.ContentEncoding;
            properties.Expiration = "10000";
            if (!string.IsNullOrEmpty(request.ContentType))
                properties.ContentType = request.ContentType;
            properties.CorrelationId = correlationId;
            if (request.Headers.Count > 0)
                properties.Headers = request.Headers;

            channel.BasicPublish(
                request.Exchange,
                request.Routing,
                false,
                properties,
                GetBody(request.ContentEncoding, request.Body)
            );
        }

        private void RegisterConsumer(string callBackId, Action<Response> recieved)
        {
            lock (consumersLock)
            {
                cunsumersCallbacks[callBackId] = recieved;
            }
        }

        private void UnregisterConsumer(string callBackId)
        {
            lock (consumersLock)
            {
                cunsumersCallbacks.Remove(callBackId);
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                consumer.Received -= OnMessage;
                if (disposing)
                {
                    channel.BasicCancel(consumer.ConsumerTag);
                    channel.Close();
                    channel.Dispose();
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
                cunsumersCallbacks.Clear();
                cunsumersCallbacks = null;
                consumer = null;
                connection = null;
                channel = null;

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        ~RabbitHole()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}