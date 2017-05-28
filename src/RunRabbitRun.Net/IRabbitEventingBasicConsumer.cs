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
        event EventHandler<BasicDeliverEventArgs> Received;
        //
        // Summary:
        //     Event fired on HandleBasicConsumeOk.
        event EventHandler<ConsumerEventArgs> Registered;
        //
        // Summary:
        //     Event fired on HandleModelShutdown.
        event EventHandler<ShutdownEventArgs> Shutdown;
        //
        // Summary:
        //     Event fired on HandleBasicCancelOk.
        event EventHandler<ConsumerEventArgs> Unregistered;
    }
}