using System;
using System.Reflection;
using RunRabbitRun.Net.Parameters;

namespace RunRabbitRun.Net.Attributes
{
    public class JsonMessageAttribute : TextMessageAttribute
    {
        public JsonMessageAttribute(
            string encoding = "utf-8") : base(encoding)
        {

        }

        public override ParameterResolver GetParameterResolver(ParameterInfo parameter)
        {
            return new JsonMessageResolver(parameter, encoding);
        }
    }
}