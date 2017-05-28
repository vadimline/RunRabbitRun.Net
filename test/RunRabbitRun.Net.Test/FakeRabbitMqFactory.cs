using FakeItEasy;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RunRabbitRun.Net.Test
{
    public static class FakeRabbitMqFactory
    {
        public static IConnection GetConnection()
        {
            return A.Fake<IConnection>();
        }

        public static IModel GetModel()
        {
            return A.Fake<IModel>();
        }

        public static IRabbitEventingBasicConsumer GetEventingBasicConsumer()
        {
            return A.Fake<IRabbitEventingBasicConsumer>();
        }
    }
}