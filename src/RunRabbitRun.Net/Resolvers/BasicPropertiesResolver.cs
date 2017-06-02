using System;
using System.Reflection;
using DryIoc;
using RabbitMQ.Client.Events;

namespace RunRabbitRun.Net.Resolvers
{
    public class BasicPropertiesResolver : ParameterResolver
    {
        public BasicPropertiesResolver(ParameterInfo parameter) : base(parameter)
        {

        }

        public override object Resolve(IContainer dependenciesContainer, BasicDeliverEventArgs messageArgs)
        {
            return messageArgs.BasicProperties;
        }
    }
}