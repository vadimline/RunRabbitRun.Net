using System;
using System.Threading;
using System.Threading.Tasks;
using RunRabbitRun.Net.Attributes;
using RunRabbitRun.Net.Sample.Models;
using RabbitMQ.Client;
using Newtonsoft.Json;

namespace RunRabbitRun.Net.Sample
{
    public class UserModule : Module
    {
        public UserModule()
        {
        }

        [Queue(exchange: "users", queue: "user-create", routingKey: "user.cmd.create", AutoDelete = true)]
        [Consume(autoAck: true)]
        [Qos(100)]
        [Channel(name: "createchannel")]
        public async Task<Response> Create(
            [Inject] IUserRepository userRepository,
            [JsonMessage] User user)
        {
            await userRepository.CreateAsync(user);
            Console.WriteLine($"Created {user.UserName}-{user.LastName}");

            return new Response
            {
                ReplyExchange = "hub",
                Body = JsonConvert.SerializeObject(new User{
                    UserName = "Pong",
                    LastName = user.LastName
                }),
                StatusCode = 200
            };
        }

        [Queue(exchange: "users", queue: "user-delete", routingKey: "user.cmd.delete", AutoDelete = true)]
        [Consume(autoAck: false)]
        [Channel(name: "deletechannel")]
        public async Task DeleteAsync(
            [Inject] IUserRepository userRepository,
            [TextMessage] string userName,
            Action ack,
            Action<bool> reject)
        {
            await userRepository.DeleteAsync(userName);
            Console.WriteLine("Deleted");
            ack();
        }
    }
}