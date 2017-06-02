using System;
using System.Reflection;
using RunRabbitRun.Net.Resolvers;

namespace RunRabbitRun.Net.Attributes
{
    public class CorrelationIdAttribute : ParameterAttribute
    {
        public CorrelationIdAttribute()
        {

        }

        public override ParameterResolver GetParameterResolver(ParameterInfo parameter)
        {
            return new CorrelationIdResolver(parameter);
        }
    }
}