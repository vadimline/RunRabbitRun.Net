using System;
using System.Threading.Tasks;

namespace RunRabbitRun.Net
{
    public interface IRabbitTunnel : IDisposable
    {
        Task<Response> SendAsync(Request request);
    }
}