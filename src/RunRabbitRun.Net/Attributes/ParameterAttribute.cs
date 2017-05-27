using System;
using System.Reflection;
using RunRabbitRun.Net.Parameters;

namespace RunRabbitRun.Net.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true)]
    public abstract class ParameterAttribute : Attribute
    {
        public abstract ParameterResolver GetParameterResolver(ParameterInfo parameter);
    }
}