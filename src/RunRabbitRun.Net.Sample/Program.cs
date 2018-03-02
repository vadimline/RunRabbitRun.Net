using System;
using System.Text;
using RunRabbitRun.Net.DryIoc;
using Newtonsoft.Json;
using RunRabbitRun.Net.Sample.Models;
using RabbitMQ.Client;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace RunRabbitRun.Net.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            var mqConnectionFactory = new RabbitMQ.Client.ConnectionFactory()
            {
                HostName = "localhost",
                Port = 5672, //5672,
                UserName = "mquser",
                Password = "1",
                VirtualHost = "rrr",
                RequestedHeartbeat = 10,
                AutomaticRecoveryEnabled = true
            };

            var connection = mqConnectionFactory.CreateConnection("RunRabbitRun.Net.Sample");

            Rabbit rabbit = new Rabbit(connection);
            rabbit.Dependencies.Register<IUserRepository, UserRepository>();
            rabbit.Run();


            // Request/Response implementation demo
            // RabbitHole rh = new RabbitHole("responses", connection);

            // var response = rh.SendAsync(new Request()
            // {
            //     Routing = "user.cmd.create",
            //     Exchange = "outgoing",
            //     ReplyRoutingKeyPrefix = "responses",
            //     Body = JsonConvert.SerializeObject(new User
            //     {
            //         UserName = "Ping",
            //         LastName = "Pong"
            //     })
            // });

            // response.Wait();

            // var res = response.Result;

            // List<Task> tasks = new List<Task>();
            // for (var i = 0; i < 1000; i++)
            // {
            //     void Send(int number)
            //     {
            //         var response = rh.SendAsync(new Request()
            //         {
            //             Routing = "user.cmd.create",
            //             Exchange = "hub",
            //             ReplyRoutingKeyPrefix = "api",
            //             Body = JsonConvert.SerializeObject(new User
            //             {
            //                 UserName = "Ping",
            //                 LastName = number.ToString()
            //             })
            //         });
            //         response.ContinueWith(tsk=>{
            //             var user = JsonConvert.DeserializeObject<User>(tsk.Result.Body);
            //             Console.WriteLine($"Expected Pong-{number} : Recieved {user.UserName}-{user.LastName}");
            //         });
            //         tasks.Add(response);
            //     };
            //     Send(i);
            // }

            // Task.WaitAll(tasks.ToArray());

            Console.WriteLine("RunRabbitRun.Net sample is running");

            while (Console.ReadLine() != "exit")
            {

            }

        }
    }
}
