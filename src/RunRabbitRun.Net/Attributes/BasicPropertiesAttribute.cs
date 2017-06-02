using System;
using System.Reflection;
using RunRabbitRun.Net.Resolvers;

namespace RunRabbitRun.Net.Attributes
{
    public class BasicPropertiesAttribute : ParameterAttribute
    {
        public BasicPropertiesAttribute()
        {
        }

        public override ParameterResolver GetParameterResolver(ParameterInfo parameter)
        {
            return new BasicPropertiesResolver(parameter);
        }
    }
}