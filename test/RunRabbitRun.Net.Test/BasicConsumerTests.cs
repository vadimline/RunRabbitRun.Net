using Xunit;

namespace RunRabbitRun.Net.Test
{
    public class BasicConsumerTests
    {
        [Fact]
        public void Should_something()
        {
            var consumer =  FakeRabbitMqFactory.GetConsumer();
            
        }
    }
}