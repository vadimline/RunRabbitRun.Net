using System;
using FakeItEasy;
using RunRabbitRun.Net.Test.Consumers;
using Xunit;
using RunRabbitRun.Net.DryIoc;
using System.Collections.Generic;

namespace RunRabbitRun.Net.Test
{
    public class PowerRabbitMQTest
    {
        [Fact]
        public void Should_declare_exchanges()
        {
            var connection = FakeRabbitMqFactory.GetConnection();
            var model = FakeRabbitMqFactory.GetModel();

            A.CallTo(() => connection.CreateModel()).Returns(model);

            RunRabbitRun.Net.Rabbit rabbit = new Rabbit(connection);
            rabbit.AutoDiscovery = false;
            rabbit.Dependencies.Register<IConsumer, Consumer>();
            rabbit.RegisterConsumer<IConsumer>();
            rabbit.Run();

            A.CallTo(() => model.ExchangeDeclare(A<string>._, A<string>._, A<bool>._, A<bool>._, A<Dictionary<string, object>>._)).MustHaveHappened();
        }

        [Fact]
        public void Should_declare_topic_exchange()
        {
            var connection = FakeRabbitMqFactory.GetConnection();
            var model = FakeRabbitMqFactory.GetModel();

            A.CallTo(() => connection.CreateModel()).Returns(model);

            RunRabbitRun.Net.Rabbit rabbit = new Rabbit(connection);
            rabbit.AutoDiscovery = false;
            rabbit.Dependencies.Register<IConsumer, Consumer>();
            rabbit.RegisterConsumer<IConsumer>();
            rabbit.Run();

            A.CallTo(() => model.ExchangeDeclare("topic", "topic", A<bool>._, A<bool>._, A<Dictionary<string, object>>._)).MustHaveHappened();
        }

        [Fact]
        public void Should_declare_direct_exchange()
        {
            var connection = FakeRabbitMqFactory.GetConnection();
            var model = FakeRabbitMqFactory.GetModel();

            A.CallTo(() => connection.CreateModel()).Returns(model);

            RunRabbitRun.Net.Rabbit rabbit = new Rabbit(connection);
            rabbit.AutoDiscovery = false;
            rabbit.Dependencies.Register<IConsumer, Consumer>();
            rabbit.RegisterConsumer<IConsumer>();
            rabbit.Run();

            A.CallTo(() => model.ExchangeDeclare("direct", "direct", A<bool>._, A<bool>._, A<Dictionary<string, object>>._)).MustHaveHappened();
        }

        [Fact]
        public void Should_declare_headers_exchange()
        {
            var connection = FakeRabbitMqFactory.GetConnection();
            var model = FakeRabbitMqFactory.GetModel();

            A.CallTo(() => connection.CreateModel()).Returns(model);

            RunRabbitRun.Net.Rabbit rabbit = new Rabbit(connection);
            rabbit.AutoDiscovery = false;
            rabbit.Dependencies.Register<IConsumer, Consumer>();
            rabbit.RegisterConsumer<IConsumer>();
            rabbit.Run();

            A.CallTo(() => model.ExchangeDeclare("headers", "headers", A<bool>._, A<bool>._, A<Dictionary<string, object>>._)).MustHaveHappened();
        }

        [Fact]
        public void Should_declare_fanout_exchange()
        {
            var connection = FakeRabbitMqFactory.GetConnection();
            var model = FakeRabbitMqFactory.GetModel();

            A.CallTo(() => connection.CreateModel()).Returns(model);

            RunRabbitRun.Net.Rabbit rabbit = new Rabbit(connection);
            rabbit.AutoDiscovery = false;
            rabbit.Dependencies.Register<IConsumer, Consumer>();
            rabbit.RegisterConsumer<IConsumer>();
            rabbit.Run();

            A.CallTo(() => model.ExchangeDeclare("fanout", "fanout", A<bool>._, A<bool>._, A<Dictionary<string, object>>._)).MustHaveHappened();
        }

        [Fact]
        public void Should_declare_durable_exchange()
        {
            var connection = FakeRabbitMqFactory.GetConnection();
            var model = FakeRabbitMqFactory.GetModel();

            A.CallTo(() => connection.CreateModel()).Returns(model);

            RunRabbitRun.Net.Rabbit rabbit = new Rabbit(connection);
            rabbit.AutoDiscovery = false;
            rabbit.Dependencies.Register<IConsumer, Consumer>();
            rabbit.RegisterConsumer<IConsumer>();
            rabbit.Run();

            A.CallTo(() => model.ExchangeDeclare("durable", A<string>._, true, A<bool>._, A<Dictionary<string, object>>._)).MustHaveHappened();
        }

        [Fact]
        public void Should_declare_notdurable_exchange()
        {
            var connection = FakeRabbitMqFactory.GetConnection();
            var model = FakeRabbitMqFactory.GetModel();

            A.CallTo(() => connection.CreateModel()).Returns(model);

            RunRabbitRun.Net.Rabbit rabbit = new Rabbit(connection);
            rabbit.AutoDiscovery = false;
            rabbit.Dependencies.Register<IConsumer, Consumer>();
            rabbit.RegisterConsumer<IConsumer>();
            rabbit.Run();

            A.CallTo(() => model.ExchangeDeclare("notdurable", A<string>._, false, A<bool>._, A<Dictionary<string, object>>._)).MustHaveHappened();
        }

        [Fact]
        public void Should_declare_autodelete_exchange()
        {
            var connection = FakeRabbitMqFactory.GetConnection();
            var model = FakeRabbitMqFactory.GetModel();

            A.CallTo(() => connection.CreateModel()).Returns(model);

            RunRabbitRun.Net.Rabbit rabbit = new Rabbit(connection);
            rabbit.AutoDiscovery = false;
            rabbit.Dependencies.Register<IConsumer, Consumer>();
            rabbit.RegisterConsumer<IConsumer>();
            rabbit.Run();

            A.CallTo(() => model.ExchangeDeclare("autodelete", A<string>._, A<bool>._, true, A<Dictionary<string, object>>._)).MustHaveHappened();
        }

        [Fact]
        public void Should_declare_notautodelete_exchange()
        {
            var connection = FakeRabbitMqFactory.GetConnection();
            var model = FakeRabbitMqFactory.GetModel();

            A.CallTo(() => connection.CreateModel()).Returns(model);

            RunRabbitRun.Net.Rabbit rabbit = new Rabbit(connection);
            rabbit.AutoDiscovery = false;
            rabbit.Dependencies.Register<IConsumer, Consumer>();
            rabbit.RegisterConsumer<IConsumer>();
            rabbit.Run();

            A.CallTo(() => model.ExchangeDeclare("notautodelete", A<string>._, A<bool>._, false, A<Dictionary<string, object>>._)).MustHaveHappened();
        }

        [Fact]
        public void Should_declare_exchange_to_exchange_binding()
        {
            var connection = FakeRabbitMqFactory.GetConnection();
            var model = FakeRabbitMqFactory.GetModel();

            A.CallTo(() => connection.CreateModel()).Returns(model);

            RunRabbitRun.Net.Rabbit rabbit = new Rabbit(connection);
            rabbit.AutoDiscovery = false;
            rabbit.Dependencies.Register<IConsumer, Consumer>();
            rabbit.RegisterConsumer<IConsumer>();
            rabbit.Run();

            A.CallTo(() => model.ExchangeBind("destination", "source", "routing.*.key", A<Dictionary<string, object>>._)).MustHaveHappened();
        }

        [Fact]
        public void Should_declare_queue()
        {
            var connection = FakeRabbitMqFactory.GetConnection();
            var model = FakeRabbitMqFactory.GetModel();

            A.CallTo(() => connection.CreateModel()).Returns(model);

            RunRabbitRun.Net.Rabbit rabbit = new Rabbit(connection);
            rabbit.AutoDiscovery = false;
            rabbit.Dependencies.Register<IConsumer, Consumer>();
            rabbit.RegisterConsumer<IConsumer>();
            rabbit.Run();

            A.CallTo(() => model.QueueDeclare(
                A<string>._,
                A<bool>._,
                A<bool>._,
                A<bool>._,
                A<Dictionary<string, object>>._)).MustHaveHappened();
        }

        [Fact]
        public void Should_declare_autodelete_queue()
        {
            var connection = FakeRabbitMqFactory.GetConnection();
            var model = FakeRabbitMqFactory.GetModel();

            A.CallTo(() => connection.CreateModel()).Returns(model);

            RunRabbitRun.Net.Rabbit rabbit = new Rabbit(connection);
            rabbit.AutoDiscovery = false;
            rabbit.Dependencies.Register<IConsumer, Consumer>();
            rabbit.RegisterConsumer<IConsumer>();
            rabbit.Run();

            A.CallTo(() => model.QueueDeclare(
                "autodelete",
                A<bool>._,
                A<bool>._,
                true,
                A<Dictionary<string, object>>._)).MustHaveHappened();
        }

        [Fact]
        public void Should_declare_notautodelete_queue()
        {
            var connection = FakeRabbitMqFactory.GetConnection();
            var model = FakeRabbitMqFactory.GetModel();

            A.CallTo(() => connection.CreateModel()).Returns(model);

            RunRabbitRun.Net.Rabbit rabbit = new Rabbit(connection);
            rabbit.AutoDiscovery = false;
            rabbit.Dependencies.Register<IConsumer, Consumer>();
            rabbit.RegisterConsumer<IConsumer>();
            rabbit.Run();

            A.CallTo(() => model.QueueDeclare(
                "notautodelete",
                A<bool>._,
                A<bool>._,
                false,
                A<Dictionary<string, object>>._)).MustHaveHappened();
        }

        [Fact]
        public void Should_declare_queue_binding()
        {
            var connection = FakeRabbitMqFactory.GetConnection();
            var model = FakeRabbitMqFactory.GetModel();

            A.CallTo(() => connection.CreateModel()).Returns(model);

            RunRabbitRun.Net.Rabbit rabbit = new Rabbit(connection);
            rabbit.AutoDiscovery = false;
            rabbit.Dependencies.Register<IConsumer, Consumer>();
            rabbit.RegisterConsumer<IConsumer>();
            rabbit.Run();

            A.CallTo(() => model.QueueBind(
                "queue",
                "exchange",
                "exchange.queue",
                A<Dictionary<string, object>>._)).MustHaveHappened();
        }

        
    }
}
