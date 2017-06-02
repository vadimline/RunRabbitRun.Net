using System;
using System.Reflection;
using RunRabbitRun.Net.Resolvers;

namespace RunRabbitRun.Net.Attributes
{
    public class RoutePrametersAttribute : ParameterAttribute
    {
        public RoutePrametersAttribute()
        {
        }

        public override ParameterResolver GetParameterResolver(ParameterInfo parameter)
        {
            return new RouteParametersResolver(parameter);
        }
    }
}