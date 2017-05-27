using System.Reflection;
using DryIoc;
using RabbitMQ.Client.Events;

namespace RunRabbitRun.Net.Parameters
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