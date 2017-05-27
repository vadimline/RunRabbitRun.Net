using System;

namespace RunRabbitRun.Net.Attributes
{
    public class ChannelAttribute : Attribute
    {
        public string Name { get; set; }
        public ChannelAttribute(string name)
        {

        }
    }
}