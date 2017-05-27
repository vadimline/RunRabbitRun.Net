using System;

namespace RunRabbitRun.Net.Attributes
{
    public class ConsumeAttribute : Attribute
    {
        public string Queue { get; private set; }

        public bool AutoAck { get; private set; }
        public ConsumeAttribute(
            string queue = null,
            bool autoAck = false)
        {
            Queue = queue;
            AutoAck = autoAck;
        }
    }
}