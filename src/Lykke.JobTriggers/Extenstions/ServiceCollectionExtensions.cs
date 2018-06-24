using System;
using System.Linq;
using JetBrains.Annotations;
using Lykke.JobTriggers.Abstractions;
using Lykke.JobTriggers.Abstractions.QueueReader;
using Lykke.JobTriggers.Implementations;
using Lykke.JobTriggers.Implementations.QueueReader;
using Lykke.JobTriggers.Triggers;
using Lykke.JobTriggers.Triggers.Bindings;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.JobTriggers.Extenstions
{
    [PublicAPI]
    public static class ServiceCollectionExtensions
    {
        private static bool HasService<T>(this IServiceCollection serviceCollection)
        {
            return serviceCollection.Any(o => o.ServiceType == typeof(T));
        }

        /// <summary>
        /// Method enables time and queue triggers
        /// </summary>        
        /// <param name="connectionPoolSetup">Used to add connections</param>
        public static void AddTriggers(this IServiceCollection serviceCollection, Action<IConnectionPool> connectionPoolSetup)
        {
            if (!serviceCollection.HasService<IPoisionQueueNotifier>())
                serviceCollection.AddTransient<IPoisionQueueNotifier, EmptyNotifier>();

            var connectionPool = new ConnectionPool();

            connectionPoolSetup(connectionPool);

            if (!connectionPool.HasConnection(ConnectionPool.DefaultConnection))
                throw new Exception("Connection pool should have default connection string");

            serviceCollection.AddSingleton<IQueueReaderFactory>(new AzureQueueReaderFactory(connectionPool));

            serviceCollection.AddTransient<QueueTriggerBinding>();
            serviceCollection.AddTransient<TimerTriggerBinding>();

            TriggerHost.UseQueueTriggers = true;
        }

        /// <summary>
        /// Method enables only time triggers
        /// </summary>
        /// <param name="serviceCollection"></param>
        public static void AddTriggers(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<QueueTriggerBinding>();
            serviceCollection.AddTransient<TimerTriggerBinding>();
        }      
    }
}

