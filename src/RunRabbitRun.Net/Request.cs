using System.Collections.Generic;

namespace RunRabbitRun.Net
{
    public class Request
    {
        public string Routing { get; set; }
        public string Body { get; set; }
        public string ContentEncoding { get; set; } = "utf-8";
        public string Exchange { get; set; }
        public string ReplyRoutingKeyPrefix { get; set; }
        public string ContentType { get; set; }
        public Dictionary<string, object> Headers { get; set; } = new Dictionary<string, object>();
    }
}