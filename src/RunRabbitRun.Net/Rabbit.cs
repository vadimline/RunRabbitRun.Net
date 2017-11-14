using System;
using System.Collections.Generic;
using System.Linq;
using RunRabbitRun.Net.DryIoc;
using RunRabbitRun.Net.Reflection;
using RabbitMQ.Client;

namespace RunRabbitRun.Net
{
    //TODO: Implemented IDisposable
    public class Rabbit
    {
        private IConnection rabbitMqConnection;
        private List<Type> consumersTypes = new List<Type>();
        private List<Consumer> consumers = new List<Consumer>();
        private readonly IContainer publicDependenciesContainer;
        private readonly IContainer internalDependenciesContainer;
        private readonly IContainer channelsContainer;

        public bool AutoDiscovery { get; set; } = true;

        public IContainer Dependencies => publicDependenciesContainer;

        public Rabbit(IConnection rabbitMqConnection)
        {
            this.rabbitMqConnection = rabbitMqConnection;

            publicDependenciesContainer = new Container();

            internalDependenciesContainer = publicDependenciesContainer.CreateFacade();
            internalDependenciesContainer.UseInstance<IConnection>(rabbitMqConnection, true);

            channelsContainer = new Container();
        }

        ///Manual consumer registration
        // Test
        public void RegisterConsumer<T>() where T : class
        {
            consumersTypes.Add(typeof(T));
        }

        public void Run()
        {
            if (AutoDiscovery)
            {
                TypeDiscovery discovery = new TypeDiscovery();
                consumersTypes = consumersTypes.Concat(discovery.DiscoverConsumers()).Distinct().ToList();
            }

            foreach (var consumerType in consumersTypes)
            {
                consumers.Add(
                    new Consumer(
                        consumerType,
                        publicDependenciesContainer.CreateFacade(),
                        internalDependenciesContainer,
                        channelsContainer));
            }
        }
    }
}