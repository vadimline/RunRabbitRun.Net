using System.Threading.Tasks;
using RunRabbitRun.Net.Attributes;
using RunRabbitRun.Net.Test.Dependencies;
using RunRabbitRun.Net.Test.Models;

namespace RunRabbitRun.Net.Test.Consumers
{
    [Consumer]
    [Exchange(name: "directexchange", Type = "direct", Durable = true, AutoDelete = false)]
    public interface IBasicConsumer
    {
        [Queue(exchange: "directexchange", queue: "user-create", routingKey: "user.cmd.create", AutoDelete = true)]
        [Consume(autoAck: true)]
        [Qos(100)]
        [Channel(name: "userschannel")]
        Task CreateAsync(
            [Inject] IUserRepository userRepository,
            [JsonMessage] User user);
    }
}