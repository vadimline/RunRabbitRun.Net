using System;
using System.Reflection;
using RunRabbitRun.Net.Resolvers;

namespace RunRabbitRun.Net.Attributes
{
    public class InjectAttribute : ParameterAttribute
    {
        public override ParameterResolver GetParameterResolver(ParameterInfo parameter)
        {
            return new InjectParameterResolver(parameter);
        }
    }
}