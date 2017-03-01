using System;
using System.Linq;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Common.Log;
using Lykke.JobTriggers.Abstractions;
using Lykke.JobTriggers.Abstractions.QueueReader;
using Lykke.JobTriggers.Implementations;
using Lykke.JobTriggers.Implementations.QueueReader;
using Lykke.JobTriggers.Triggers;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.JobTriggers.Extenstions
{
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
            if (!serviceCollection.HasService<ILog>())
                serviceCollection.AddSingleton<ILog, LogToConsole>();

            if (!serviceCollection.HasService<IPoisionQueueNotifier>())
                serviceCollection.AddTransient<IPoisionQueueNotifier, EmptyNotifier>();

            var connectionPool = new ConnectionPool();

            connectionPoolSetup(connectionPool);

            if (!connectionPool.HasConnection(ConnectionPool.DefaultConnection))
                throw new Exception("Connection pool should have default connection string");

            serviceCollection.AddSingleton<IQueueReaderFactory>(new AzureQueueReaderFactory(connectionPool));

            TriggerHost.UseQueueTriggers = true;
        }

        /// <summary>
        /// Method enables only time triggers
        /// </summary>
        /// <param name="serviceCollection"></param>
        public static void AddTriggers(this IServiceCollection serviceCollection)
        {
            if (!serviceCollection.HasService<ILog>())
                serviceCollection.AddSingleton<ILog, LogToConsole>();
        }      
    }
}
