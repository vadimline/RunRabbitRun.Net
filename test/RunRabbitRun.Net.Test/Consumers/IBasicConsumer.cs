using System;
using System.Threading.Tasks;
using RunRabbitRun.Net.Attributes;
using RunRabbitRun.Net.Test.Dependencies;
using RunRabbitRun.Net.Test.Models;

namespace RunRabbitRun.Net.Test.Consumers
{
    [Consumer]
    public interface IBasicConsumer
    {
        [Consume(queue: "queue", autoAck: true)]
        Task ShouldInject([Inject] IUserRepository userRepository);

        [Consume(queue: "queue", autoAck: true)]
        Task ShouldInjectRawBytesMessage([Message]byte[] message);

        [Consume(queue: "queue", autoAck: true)]
        Task ShouldInjectTextMessage([TextMessage]string message);

        [Consume(queue: "queue", autoAck: true)]
        Task ShouldInjectDeserializedJsonMessage([JsonMessage]User message);

        [Consume(queue: "queue", autoAck: false)]
        Task ShouldInjectAckAndNoAckCallbacks(Action ack, Action<bool> noack);
    }
}