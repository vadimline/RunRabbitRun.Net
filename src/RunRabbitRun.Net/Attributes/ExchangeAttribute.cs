using System;

namespace RunRabbitRun.Net.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
    public class ExchangeAttribute : Attribute
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public bool Durable { get; set; }
        public bool AutoDelete { get; set; }

        public string AlternateExchange { get; set; }

        public ExchangeAttribute(string name)
        {
            Name = name;
        }
    }
}