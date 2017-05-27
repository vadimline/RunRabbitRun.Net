using System;

namespace RunRabbitRun.Net.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
    public class ExchangeToExchangeBindingAttribute : Attribute
    {
        public string Source { get; private set; }

        public string Destination { get; private set; }

        public string RoutingKey { get; private set; }

        public ExchangeToExchangeBindingAttribute(
            string source, 
            string destination, 
            string routingKey)
        {
            Source = source;
            Destination = destination;
            RoutingKey = routingKey;
        }
    }
}
