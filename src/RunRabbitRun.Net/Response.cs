using System.Collections.Generic;

namespace RunRabbitRun.Net
{
    public class Response
    {
        public string Body { get; set; }
        public Dictionary<string, object> Headers { get; set; } = new Dictionary<string, object>();
        public int StatusCode
        {
            get
            {
                return (int)Headers["StatusCode"];
            }
            set
            {
                Headers["StatusCode"] = value;
            }
        }

        public string StatusText
        {
            get
            {
                return Headers["StatusText"] as string;
            }
            set
            {
                Headers["StatusText"] = value;
            }
        }
        public string ReplyTo { get; set; }
        public string ContentEncoding { get; set; } = "utf-8";
        public string ReplyExchange { get; set; }
    }
}