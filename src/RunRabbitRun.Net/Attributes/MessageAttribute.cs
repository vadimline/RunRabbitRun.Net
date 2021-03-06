using System;
using System.Reflection;
using RunRabbitRun.Net.Resolvers;

namespace RunRabbitRun.Net.Attributes
{
    public class MessageAttribute : ParameterAttribute
    {
        public MessageAttribute()
        {

        }

        public override ParameterResolver GetParameterResolver(ParameterInfo parameter)
        {
            return new MessageParameterResolver(parameter);
        }
    }
}