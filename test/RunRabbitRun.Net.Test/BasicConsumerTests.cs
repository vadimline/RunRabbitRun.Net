using FakeItEasy;
using Xunit;
using DryIoc;
using RunRabbitRun.Net.Test.Consumers;
using RabbitMQ.Client.Events;
using RunRabbitRun.Net.Test.Dependencies;
using System.Text;
using System.Linq;
using RunRabbitRun.Net.Test.Models;
using Newtonsoft.Json;
using System;

namespace RunRabbitRun.Net.Test
{
    public class BasicConsumerTests
    {
        [Fact]
        public void Should_inject_dependency()
        {
            var rmqConsumer = FakeRabbitMqFactory.GetEventingBasicConsumer();
            var connection = FakeRabbitMqFactory.GetConnection();

            var consumer = A.Fake<IBasicConsumer>();
            var dependecy = new UserRepository();

            RunRabbitRun.Net.Rabbit rabbit = new Rabbit(connection);
            rabbit.AutoDiscovery = false;
            rabbit.Dependencies.RegisterDelegate<IUserRepository>(r => dependecy);
            rabbit.Dependencies.RegisterDelegate<IRabbitEventingBasicConsumer>(r => rmqConsumer);
            rabbit.Dependencies.RegisterDelegate<IBasicConsumer>(r => consumer);
            rabbit.RegisterConsumer<IBasicConsumer>();
            rabbit.Run();

            var args = new BasicDeliverEventArgs();

            rmqConsumer.Received += Raise.With<AsyncEventHandler<BasicDeliverEventArgs>>(null, args);

            A.CallTo(() =>
                consumer.ShouldInject(dependecy)).MustHaveHappened();
        }

        [Fact]
        public void Should_inject_rawbytes()
        {
            var rmqConsumer = FakeRabbitMqFactory.GetEventingBasicConsumer();
            var connection = FakeRabbitMqFactory.GetConnection();
            var consumer = A.Fake<IBasicConsumer>();

            RunRabbitRun.Net.Rabbit rabbit = new Rabbit(connection);
            rabbit.AutoDiscovery = false;
            rabbit.Dependencies.RegisterDelegate<IRabbitEventingBasicConsumer>(r => rmqConsumer);
            rabbit.Dependencies.RegisterDelegate<IBasicConsumer>(r => consumer);
            rabbit.RegisterConsumer<IBasicConsumer>();
            rabbit.Run();

            byte[] message = Encoding.UTF8.GetBytes("textmessage"); ;
            var args = new BasicDeliverEventArgs();
            args.Body = message;
            rmqConsumer.Received += Raise.With<AsyncEventHandler<BasicDeliverEventArgs>>(null, args);

            A.CallTo(() => consumer
            .ShouldInjectRawBytesMessage(A<byte[]>
                .That
                .Matches(x => x.SequenceEqual(message))))
            .MustHaveHappened();
        }

        [Fact]
        public void Should_inject_text_message()
        {
            var rmqConsumer = FakeRabbitMqFactory.GetEventingBasicConsumer();
            var connection = FakeRabbitMqFactory.GetConnection();
            var consumer = A.Fake<IBasicConsumer>();

            RunRabbitRun.Net.Rabbit rabbit = new Rabbit(connection);
            rabbit.AutoDiscovery = false;
            rabbit.Dependencies.RegisterDelegate<IRabbitEventingBasicConsumer>(r => rmqConsumer);
            rabbit.Dependencies.RegisterDelegate<IBasicConsumer>(r => consumer);
            rabbit.RegisterConsumer<IBasicConsumer>();
            rabbit.Run();

            byte[] message = Encoding.UTF8.GetBytes("textmessage"); ;
            var args = new BasicDeliverEventArgs();
            args.Body = message;
            rmqConsumer.Received += Raise.With<AsyncEventHandler<BasicDeliverEventArgs>>(null, args);

            A.CallTo(() => consumer
            .ShouldInjectTextMessage(A<string>
                .That
                .Matches(x => x == "textmessage")))
            .MustHaveHappened();
        }

        [Fact]
        public void Should_inject_deserialized_json_message()
        {
            var rmqConsumer = FakeRabbitMqFactory.GetEventingBasicConsumer();
            var connection = FakeRabbitMqFactory.GetConnection();
            var consumer = A.Fake<IBasicConsumer>();

            RunRabbitRun.Net.Rabbit rabbit = new Rabbit(connection);
            rabbit.AutoDiscovery = false;
            rabbit.Dependencies.RegisterDelegate<IRabbitEventingBasicConsumer>(r => rmqConsumer);
            rabbit.Dependencies.RegisterDelegate<IBasicConsumer>(r => consumer);
            rabbit.RegisterConsumer<IBasicConsumer>();
            rabbit.Run();

            User user = new User();
            user.FirstName = "UserFirstName";
            user.LastName = "UserLastName";
            user.UserName = "UserName";
            byte[] message = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(user));
            var args = new BasicDeliverEventArgs();
            args.Body = message;
            rmqConsumer.Received += Raise.With<AsyncEventHandler<BasicDeliverEventArgs>>(null, args);

            A.CallTo(() => consumer
            .ShouldInjectDeserializedJsonMessage(A<User>
                .That
                .Matches(x => x.FirstName == user.FirstName
                    && x.LastName == user.LastName
                    && x.UserName == user.UserName)))
            .MustHaveHappened();
        }

        [Fact]
        public void Should_inject_ack_and_noack_callbacks()
        {
            var rmqConsumer = FakeRabbitMqFactory.GetEventingBasicConsumer();
            var connection = FakeRabbitMqFactory.GetConnection();
            var consumer = A.Fake<IBasicConsumer>();

            RunRabbitRun.Net.Rabbit rabbit = new Rabbit(connection);
            rabbit.AutoDiscovery = false;
            rabbit.Dependencies.RegisterDelegate<IRabbitEventingBasicConsumer>(r => rmqConsumer);
            rabbit.Dependencies.RegisterDelegate<IBasicConsumer>(r => consumer);
            rabbit.RegisterConsumer<IBasicConsumer>();
            rabbit.Run();

            byte[] message = Encoding.UTF8.GetBytes("message");
            var args = new BasicDeliverEventArgs();
            args.Body = message;
            rmqConsumer.Received += Raise.With<AsyncEventHandler<BasicDeliverEventArgs>>(null, args);

            A.CallTo(() => consumer
            .ShouldInjectAckAndNoAckCallbacks(A<Action>
                .That.IsNotNull(), A<Action<bool>>.That.IsNotNull()))
            .MustHaveHappened();
        }
    }
}