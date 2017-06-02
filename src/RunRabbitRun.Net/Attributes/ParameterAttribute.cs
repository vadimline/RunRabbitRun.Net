using System;
using System.Reflection;
using RunRabbitRun.Net.Resolvers;

namespace RunRabbitRun.Net.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public abstract class ParameterAttribute : Attribute
    {
        public abstract ParameterResolver GetParameterResolver(ParameterInfo parameter);
    }
}