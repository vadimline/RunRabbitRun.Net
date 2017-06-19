using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RunRabbitRun.Net
{
    //We need this class so we can mock events
    public class RabbitEventingBasicConsumer : AsyncEventingBasicConsumer, IRabbitEventingBasicConsumer
    {
        //     Constructor which sets the Model property to the /// given value.
        public RabbitEventingBasicConsumer(IModel model) : base(model)
        {

        }

        //
        // Summary:
        //     Event fired on HandleBasicDeliver.
        public new event AsyncEventHandler<BasicDeliverEventArgs> Received
        {
            add
            {
                base.Received += value;
            }
            remove
            {
                base.Received -= value;
            }
        }
        //
        // Summary:
        //     Event fired on HandleBasicConsumeOk.
        public new event AsyncEventHandler<ConsumerEventArgs> Registered
        {
            add
            {
                base.Registered += value;
            }
            remove
            {
                base.Registered -= value;
            }
        }

        //
        // Summary:
        //     Event fired on HandleModelShutdown.
        public new event AsyncEventHandler<ShutdownEventArgs> Shutdown
        {
            add
            {
                base.Shutdown += value;
            }
            remove
            {
                base.Shutdown -= value;
            }
        }

        //
        // Summary:
        //     Event fired on HandleBasicCancelOk.
        public new event AsyncEventHandler<ConsumerEventArgs> Unregistered
        {
            add
            {
                base.Unregistered += value;
            }
            remove
            {
                base.Unregistered -= value;
            }
        }
    }
}