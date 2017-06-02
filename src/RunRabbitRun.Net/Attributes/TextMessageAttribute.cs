using System;
using System.Reflection;
using RunRabbitRun.Net.Resolvers;

namespace RunRabbitRun.Net.Attributes
{
    public class TextMessageAttribute : MessageAttribute
    {
        protected string encoding = "utf-8";
        public TextMessageAttribute(string encoding = "utf-8")
        {
            this.encoding = encoding;
        }

        public override ParameterResolver GetParameterResolver(ParameterInfo parameter)
        {
            return new TextMessageResolver(parameter, encoding);
        }
    }
}