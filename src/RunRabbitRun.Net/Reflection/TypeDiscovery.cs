using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyModel;
using RunRabbitRun.Net.Attributes;

namespace RunRabbitRun.Net.Reflection
{
    public class TypeDiscovery
    {
        private static string ProwerRabbitMqAssemblyName = typeof(Rabbit).GetTypeInfo().Assembly.GetName().Name;
        private Assembly entryAssembly;
        private DependencyContext dependencyContext;

        public TypeDiscovery() :
            this(Assembly.GetEntryAssembly())
        {
        }

        public TypeDiscovery(Assembly entryAssembly)
        {
            this.entryAssembly = Assembly.GetEntryAssembly();
            dependencyContext = DependencyContext.Load(entryAssembly);
        }

        public Type[] DiscoverConsumers()
        {
            List<Type> consumers = new List<Type>();

            foreach (Assembly assembly in GetAssemblies())
            {
                var assemblyTypes = assembly.GetTypes();

                consumers
                    .AddRange(assemblyTypes
                        .Where(type =>
                        {
                            var typeInfo = type.GetTypeInfo();
                            return !typeInfo.IsAbstract &&
                            typeInfo.GetCustomAttribute(typeof(ConsumerAttribute), true) != null;
                        }));
            }

            return consumers.ToArray();
        }

        public Assembly[] GetAssemblies()
        {
            List<Assembly> assemblies = new List<Assembly>();
            assemblies.Add(entryAssembly);
            foreach (var runtimeLibrary in dependencyContext.RuntimeLibraries)
            {
                if (!IsReferencingPowerRabbitMq(runtimeLibrary))
                    continue;

                foreach (var runtimeAssembly in runtimeLibrary.Assemblies)
                {
                    assemblies.Add(Assembly.Load(runtimeAssembly.Name));
                }
            }
            return assemblies.ToArray();
        }

        public bool IsReferencingPowerRabbitMq(RuntimeLibrary runtimeLibrary)
        {
            return runtimeLibrary.Dependencies.Any(dependency => dependency.Name == ProwerRabbitMqAssemblyName);
        }
    }
}