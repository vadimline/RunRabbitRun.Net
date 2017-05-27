using System;
using System.Threading.Tasks;
using RunRabbitRun.Net.Attributes;
using RunRabbitRun.Net.Test.Dependencies;
using RunRabbitRun.Net.Test.Models;

namespace RunRabbitRun.Net.Test.Consumers
{
    public class Consumer : IConsumer
    {
        public Task OnMessageFromAutoDelete([Inject] IUserRepository userRepository, [JsonMessage("utf-8")] User user)
        {
            throw new NotImplementedException();
        }

        public Task OnMessageFromNotAutoDelete([Inject] IUserRepository userRepository, [JsonMessage("utf-8")] User user)
        {
            throw new NotImplementedException();
        }

        public Task OnMessageFromQueue([Inject] IUserRepository userRepository, [JsonMessage("utf-8")] User user)
        {
            throw new NotImplementedException();
        }
    }
}