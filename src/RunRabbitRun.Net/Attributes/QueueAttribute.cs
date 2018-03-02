using System;

namespace RunRabbitRun.Net.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class QueueAttribute : Attribute
    {
        public string Exchange { get; private set; }

        public string Queue { get; private set; }

        public string[] RoutingKey { get; private set; }

        public bool Durable { get; set; }

        public bool AutoDelete { get; set; }

        public bool Exclusive { get; set; }

        public QueueAttribute(
            string exchange,
            string queue,
            params string[] routingKey)
        {
            Exchange = exchange;
            Queue = queue;
            RoutingKey = routingKey;
        }

        public QueueAttribute(
            string exchange,
            string queue,
            string routingKey)
        {
            Exchange = exchange;
            Queue = queue;
            RoutingKey = new string[] { routingKey };
        }
    }
}