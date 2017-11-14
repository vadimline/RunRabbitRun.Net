using System.Reflection;
using RunRabbitRun.Net.DryIoc;
using Newtonsoft.Json;
using RabbitMQ.Client.Events;

namespace RunRabbitRun.Net.Resolvers
{
    public class JsonMessageResolver : TextMessageResolver
    {
        public JsonMessageResolver(ParameterInfo parameter, string encoding) : base(parameter, encoding)
        {
        }

        public override object Resolve(IContainer dependenciesContainer, BasicDeliverEventArgs messageArgs)
        {
            var message = base.Resolve(dependenciesContainer, messageArgs) as string;
            return JsonConvert.DeserializeObject(message, Parameter.ParameterType);
        }
    }
}