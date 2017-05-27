using System.Reflection;
using DryIoc;
using RabbitMQ.Client.Events;

namespace RunRabbitRun.Net.Parameters
{
    public class TextMessageResolver : MessageParameterResolver
    {
        protected string ContentEncoding { get; private set; }
        public TextMessageResolver(
            ParameterInfo parameter,
            string encoding) : base(parameter)
        {
            this.ContentEncoding = encoding;
        }

        public override object Resolve(IContainer dependenciesContainer, BasicDeliverEventArgs messageArgs)
        {
            var message = base.Resolve(dependenciesContainer, messageArgs) as byte[];
            var encoding = System.Text.Encoding.GetEncoding(ContentEncoding);
            return encoding.GetString(message);
        }
    }
}