using RabbitMQ.Client;

namespace RunRabbitRun.Net
{
    public class RabbitHole : IRabbitHole
    {
        private IConnection connection;
        public RabbitHole(IConnection connection)
        {
            this.connection = connection;
        }

        public void Send(Envelope envelope)
        {
            SendInternal(envelope);
        }

        public void Send(string exchange, string routingKey, string body)
        {
            SendInternal(new Envelope()
            {
                Body = body,
                Exchange = exchange,
                ContentType = "text/plain",
                Routing = routingKey
            });
        }

        private void SendInternal(Envelope envelope)
        {
            using (IModel model = connection.CreateModel())
            {
                var properties = model.CreateBasicProperties();
                properties.CorrelationId = envelope.CorrelationId;
                properties.ContentType = envelope.ContentType;
                properties.ContentEncoding = envelope.ContentEncoding;
                if (envelope.Expiration > 0)
                    properties.Expiration = envelope.Expiration.ToString();
                properties.Headers = envelope.Headers;

                model.BasicPublish(envelope.Exchange
                , envelope.Routing
                , false
                , properties
                , GetBody(envelope.ContentEncoding, envelope.Body));
            }
        }

        private byte[] GetBody(string contentEncoding, string body)
        {
            var encoding = System.Text.Encoding.GetEncoding(contentEncoding);
            return encoding.GetBytes(body);
        }
    }
}