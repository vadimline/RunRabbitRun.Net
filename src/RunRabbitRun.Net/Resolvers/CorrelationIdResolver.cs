using System;
using System.Reflection;
using RunRabbitRun.Net.DryIoc;
using RabbitMQ.Client.Events;

namespace RunRabbitRun.Net.Resolvers
{
    public class CorrelationIdResolver : ParameterResolver
    {
        public CorrelationIdResolver(ParameterInfo parameter) : base(parameter)
        {

        }

        public override object Resolve(IContainer dependenciesContainer, BasicDeliverEventArgs messageArgs)
        {
            return messageArgs.BasicProperties.CorrelationId;
        }
    }
}