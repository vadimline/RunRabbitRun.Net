# RunRabbitRun.Net

.Net client to work with RabbitMq in declarative way using attributes

Use official RabbitMQ.Client and DryIoc for dependency injection.

# Warning

**Project not ready for production**

# Code Example (Basic consumer)

```cs

[Consumer]
[Exchange(name: "hub", Type = "topic", Durable = true, AutoDelete = false)]
[Exchange(name: "users", Type = "topic", Durable = true, AutoDelete = false)]
[ExchangeToExchangeBinding(source: "hub", destination: "users", routingKey: "user.#")]

public class UserConsumer
{
        [Queue(exchange: "users", queue: "users-create", routingKey: "users.cmd.create", AutoDelete = false)]
        [Qos(100)]
        [Consume(autoAck: true)]
        [Channel(name: "userschannel")]
        public async Task DeleteAsync(
            [Inject] IUserRepository userRepository,
            [JsonMessage] User userName)
        {
            await userRepository.CreateAsync(userName);
        }

        [Queue(exchange: "users", queue: "users-delete", routingKey: "users.cmd.delete", AutoDelete = false)]
        [Consume(autoAck: false)]
        [Channel(name: "userschannel")]
        public async Task DeleteAsync(
            [Inject] IUserRepository userRepository,
            [TextMessage] string userName,
            Action ack,
            Action<bool> reject)
        {
            await userRepository.DeleteAsync(userName);
            ack();
        }
}



static void Main(string[] args)
{

    var mqConnectionFactory = new RabbitMQ.Client.ConnectionFactory()
    {
        HostName = "localhost",
        Port = 5672,
        UserName = "guest",
        Password = "guest",
        VirtualHost = "/",
        RequestedHeartbeat = 10,
        AutomaticRecoveryEnabled = true
    };

    var connection = mqConnectionFactory.CreateConnection("RunRabbitRun.Net.Sample");

    Rabbit rabbit = new Rabbit(connection);
    rabbit.Dependencies.Register<IUserRepository, UserRepository>();
    rabbit.Run();
}

```

# Code Example (Request/Response consumer)

```
Comming soon
```

# Installation
NUGET Comming soon


# License
MIT