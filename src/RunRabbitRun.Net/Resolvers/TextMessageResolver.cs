using System.Reflection;
using RunRabbitRun.Net.DryIoc;
using RabbitMQ.Client.Events;

namespace RunRabbitRun.Net.Resolvers
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