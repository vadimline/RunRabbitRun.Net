using System;
using System.Reflection;
using DryIoc;
using RabbitMQ.Client.Events;

namespace RunRabbitRun.Net.Resolvers
{
    public class MessageParameterResolver : ParameterResolver
    {
        public MessageParameterResolver(ParameterInfo parameter) : base(parameter)
        {
        }

        public override object Resolve(IContainer dependenciesContainer, BasicDeliverEventArgs messageArgs)
        {
            return messageArgs.Body;
        }
    }
}