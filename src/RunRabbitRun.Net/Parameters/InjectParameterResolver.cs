using System;
using System.Reflection;
using DryIoc;
using RabbitMQ.Client.Events;

namespace RunRabbitRun.Net.Parameters
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