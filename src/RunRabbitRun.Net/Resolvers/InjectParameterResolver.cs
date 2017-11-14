using System;
using System.Reflection;
using RunRabbitRun.Net.DryIoc;
using RabbitMQ.Client.Events;

namespace RunRabbitRun.Net.Resolvers
{
    public class InjectParameterResolver : ParameterResolver
    {
        public InjectParameterResolver(ParameterInfo parameter) : base(parameter)
        {

        }

        public override object Resolve(IContainer dependenciesContainer, BasicDeliverEventArgs messageArgs)
        {
            return dependenciesContainer.Resolve(Parameter.ParameterType);
        }
    }
}