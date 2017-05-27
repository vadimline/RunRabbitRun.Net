using System.Threading.Tasks;
using RunRabbitRun.Net.Attributes;
using RunRabbitRun.Net.Test.Dependencies;
using RunRabbitRun.Net.Test.Models;

namespace RunRabbitRun.Net.Test.Consumers
{
    [Consumer]
    [Exchange(name: "fanout", Type = "fanout", Durable = true, AutoDelete = false)]
    [Exchange(name: "headers", Type = "headers", Durable = true, AutoDelete = false)]
    [Exchange(name: "direct", Type = "direct", Durable = true, AutoDelete = false)]
    [Exchange(name: "topic", Type = "topic", Durable = true, AutoDelete = false)]
    [Exchange(name: "durable", Type = "topic", Durable = true, AutoDelete = false)]
    [Exchange(name: "notdurable", Type = "topic", Durable = false, AutoDelete = false)]
    [Exchange(name: "notautodelete", Type = "topic", Durable = false, AutoDelete = false)]
    [Exchange(name: "autodelete", Type = "topic", Durable = false, AutoDelete = true)]
    [Exchange(name: "source", Type = "fanout", Durable = true, AutoDelete = false)]
    [Exchange(name: "destination", Type = "fanout", Durable = true, AutoDelete = false)]
    [ExchangeToExchangeBinding(source: "source", destination: "destination", routingKey: "routing.*.key")]
    public interface IConsumer
    {
        [Queue(exchange: "exchange", queue: "autodelete", routingKey: "exchange.autodelete", AutoDelete = true)]
        [Consume(queue: "autodelete", autoAck: true)]
        [Qos(100)]
        [Channel(name: "channel1")]
        Task OnMessageFromAutoDelete(
            [Inject] IUserRepository userRepository,
            [JsonMessage] User user);


        [Queue(exchange: "exchange", queue: "notautodelete", routingKey: "exchange.notautodelete", AutoDelete = false)]
        [Consume(queue: "notautodelete", autoAck: false)]
        [Qos(100)]
        [Channel(name: "channel1")]
        Task OnMessageFromNotAutoDelete(
            [Inject] IUserRepository userRepository,
            [JsonMessage] User user);

        [Queue(exchange: "exchange", queue: "queue", routingKey: "exchange.queue", AutoDelete = false)]
        [Consume(queue: "notautodelete", autoAck: false)]
        [Qos(100)]
        [Channel(name: "queuechannel")]
        Task OnMessageFromQueue(
            [Inject] IUserRepository userRepository,
            [JsonMessage] User user);
    }
}