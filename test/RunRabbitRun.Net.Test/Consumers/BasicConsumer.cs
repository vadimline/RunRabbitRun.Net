using System;
using System.Threading.Tasks;
using RunRabbitRun.Net.Attributes;
using RunRabbitRun.Net.Test.Dependencies;
using RunRabbitRun.Net.Test.Models;

namespace RunRabbitRun.Net.Test.Consumers
{
    public class BasicConsumer : IBasicConsumer
    {
        public Task CreateAsync([Inject] IUserRepository userRepository, [JsonMessage("utf-8")] User user)
        {
            throw new NotImplementedException();
        }
    }
}