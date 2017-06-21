using System;
using System.Collections.Generic;

namespace RunRabbitRun.Net
{
    public class Request : Envelope
    {
        public Request()
        {
            Expiration = 30000;
        }
    }
}