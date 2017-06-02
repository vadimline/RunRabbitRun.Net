using System;
using System.Reflection;
using DryIoc;
using RabbitMQ.Client.Events;

namespace RunRabbitRun.Net.Resolvers
{
    public class RouteParametersResolver : ParameterResolver
    {
        public RouteParametersResolver(ParameterInfo parameter) : base(parameter)
        {
            throw new NotImplementedException();
        }

        public override object Resolve(IContainer dependenciesContainer, BasicDeliverEventArgs messageArgs)
        {
            throw new NotImplementedException();
        }
    }
}