using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RunRabbitRun.Net
{
    public interface IRabbitEventingBasicConsumer : IBasicConsumer
    {
        //
        // Summary:
        //     Event fired on HandleBasicDeliver.
        event AsyncEventHandler<BasicDeliverEventArgs> Received;
        //
        // Summary:
        //     Event fired on HandleBasicConsumeOk.
        event AsyncEventHandler<ConsumerEventArgs> Registered;
        //
        // Summary:
        //     Event fired on HandleModelShutdown.
        event AsyncEventHandler<ShutdownEventArgs> Shutdown;
        //
        // Summary:
        //     Event fired on HandleBasicCancelOk.
        event AsyncEventHandler<ConsumerEventArgs> Unregistered;
    }
}