using System.Reflection;
using RunRabbitRun.Net.DryIoc;
using RabbitMQ.Client.Events;

namespace RunRabbitRun.Net.Resolvers
{
    public abstract class ParameterResolver
    {
        protected ParameterInfo Parameter { get; private set; }
        public ParameterResolver(ParameterInfo parameter)
        {
            Parameter = parameter;
        }
        public abstract object Resolve(IContainer dependenciesContainer, BasicDeliverEventArgs messageArgs);
    }
}