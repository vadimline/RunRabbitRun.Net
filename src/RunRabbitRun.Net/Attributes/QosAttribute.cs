using System;

namespace RunRabbitRun.Net.Attributes
{
    public class QosAttribute : Attribute
    {
        public ushort PrefetchCount { get; private set; }
        public QosAttribute( ushort prefetchCount)
        {
            PrefetchCount = prefetchCount;
        }
    }
}