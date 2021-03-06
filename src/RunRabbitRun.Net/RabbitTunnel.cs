using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RunRabbitRun.Net
{
    public class RabbitTunnel : IRabbitTunnel
    {
        private IConnection connection;
        private string replyExchange;
        private string replyRoute;
        private string rabbitTunnelId;
        private string rabbitTunnelQueueName;
        private EventingBasicConsumer consumer;
        private Dictionary<string, Action<Response>> cunsumersCallbacks = new Dictionary<string, Action<Response>>();
        private object consumersCallbackLock = new object();
        private object publishLock = new object();
        private bool initialized = false;
        private IModel channel;
        public IModel Channel
        {
            get
            {
                return channel;
            }
            set => channel = value;
        }
        public RabbitTunnel(
            string replyExchange,
            string replyRoute,
            string tunnelName,
            IConnection connection)
        {
            this.connection = connection;
            this.replyExchange = replyExchange;
            rabbitTunnelId = Guid.NewGuid().ToString();
            this.replyRoute = $"{replyRoute}.{rabbitTunnelId}";
            rabbitTunnelQueueName = $"rtunnel-{tunnelName}-{rabbitTunnelId}";
            Setup();
        }
        private void Setup()
        {
            Channel = connection.CreateModel();
            Channel.BasicReturn += OnReturn;
            Channel.QueueDeclare(rabbitTunnelQueueName, false, true, true, null);
            Channel.QueueBind(rabbitTunnelQueueName, replyExchange, replyRoute);
            consumer = new EventingBasicConsumer(Channel);
            consumer.Received += OnMessageReceived;
            Channel.BasicConsume(rabbitTunnelQueueName, true, consumer);
            initialized = true;
        }

        private void OnReturn(object sender, BasicReturnEventArgs args)
        {
            if (!cunsumersCallbacks.ContainsKey(args.BasicProperties.CorrelationId))
                return;

            Action<Response> consumer = null;

            cunsumersCallbacks.TryGetValue(args.BasicProperties.CorrelationId, out consumer);
            if (consumer == null)
                return;

            Response response = new Response();
            response.StatusCode = args.ReplyCode;
            response.StatusText = args.ReplyText;
            consumer(response);
        }

        private void OnMessageReceived(object sender, BasicDeliverEventArgs args)
        {
            if (!args.BasicProperties.IsHeadersPresent())
                return;

            if (!args.BasicProperties.Headers.ContainsKey("rabbit-callback-id"))
                return;

            var callBackId = Encoding.UTF8.GetString((byte[])args.BasicProperties.Headers["rabbit-callback-id"]) as string;

            if (!cunsumersCallbacks.ContainsKey(callBackId))
                return;

            Action<Response> consumer = null;
            cunsumersCallbacks.TryGetValue(callBackId, out consumer);
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
            if (args.Body == null)
                return null;

            var encoding = System.Text.Encoding.GetEncoding(args.BasicProperties.ContentEncoding);
            return encoding.GetString(args.Body);
        }

        //TODO: Same logic is used in multiple places, consider move it to class
        private byte[] GetBody(string contentEncoding, string body)
        {
            var encoding = System.Text.Encoding.GetEncoding(contentEncoding);
            return encoding.GetBytes(body);
        }

        public Task<Response> SendAsync(Request request)
        {
            var delayCancelToken = new CancellationTokenSource();

            TaskCompletionSource<Response> taskCompletionSource =
                new TaskCompletionSource<Response>();

            void ResponseRecieved(Response r)
            {
                taskCompletionSource.SetResult(r);
                delayCancelToken.Cancel();
            }

            var callBackId = Guid.NewGuid().ToString();
            RegisterCallback(callBackId, ResponseRecieved);
            Send(callBackId, request);

            Task.Delay(request.Expiration == -1 ? 10000 : request.Expiration, delayCancelToken.Token).ContinueWith(task =>
              {
                  UnregisterCallback(request.CorrelationId);
                  if (!task.IsCanceled)
                      // This mean that delay task complete and no response recieved
                      taskCompletionSource.SetResult(new Response
                      {
                          StatusCode = 408,
                          StatusText = "Timeout"
                      });
              });

            return taskCompletionSource.Task;
        }

        private void Send(string callBackId, Request request)
        {
            var properties = Channel.CreateBasicProperties();
            properties.ReplyTo = replyRoute;
            properties.ContentEncoding = request.ContentEncoding;

            int expiration = request.Expiration == -1 ? 10000 : request.Expiration;
            properties.Expiration = expiration.ToString();

            if (!string.IsNullOrEmpty(request.ContentType))
                properties.ContentType = request.ContentType;

            properties.CorrelationId = request.CorrelationId;
            if (request.Headers.Count > 0)
                properties.Headers = request.Headers;
            else
                properties.Headers = new Dictionary<string, object>();

            properties.Headers.Add("rabbit-callback-id", callBackId);

            //Since IModel not thread safe we will lock publishing until official client will not support async
            lock (publishLock)
            {
                Channel.BasicPublish(
                    request.Exchange,
                    request.Routing,
                    true,
                    properties,
                    GetBody(request.ContentEncoding, request.Body)
                );
            }
        }

        private void RegisterCallback(string callBackId, Action<Response> recieved)
        {
            lock (consumersCallbackLock)
            {
                cunsumersCallbacks[callBackId] = recieved;
            }
        }

        private void UnregisterCallback(string callBackId)
        {
            lock (consumersCallbackLock)
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
                if (initialized)
                {
                    consumer.Received -= OnMessageReceived;
                    channel.BasicReturn -= OnReturn;
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
                }


                cunsumersCallbacks = null;
                consumer = null;
                connection = null;
                channel = null;

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        ~RabbitTunnel()
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