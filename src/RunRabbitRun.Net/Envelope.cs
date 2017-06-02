using System;
using System.Collections.Generic;

namespace RunRabbitRun.Net
{
    public class Envelope
    {
        public string CorrelationId { get; set; } = Guid.NewGuid().ToString();
        public string Routing { get; set; }
        public string Body { get; set; }
        public string Exchange { get; set; }
        public string ContentType { get; set; }
        public string ContentEncoding { get; set; } = "utf-8";
        public int Expiration { get; set; } = -1;
        public Dictionary<string, object> Headers { get; set; } = new Dictionary<string, object>();
    }
}