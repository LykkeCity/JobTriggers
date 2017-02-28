using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Common;
using Common.Log;
using LykkeTriggers.Abstractions;
using LykkeTriggers.Abstractions.QueueReader;
using LykkeTriggers.Implementations;
using LykkeTriggers.Implementations.QueueReader;
using LykkeTriggers.Triggers;
using Microsoft.Extensions.DependencyInjection;

namespace LykkeTriggers.Extenstions
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

        /// <summary>
        /// Method enables time and queue triggers
        /// </summary>        
        /// <param name="connectionPoolSetup">Used to add connections</param>
        public static void AddTriggers(this ContainerBuilder containerBuilder, Action<IConnectionPool> connectionPoolSetup)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddTriggers(connectionPoolSetup);
            containerBuilder.Populate(serviceCollection);
        }



        /// <summary>
        /// Method enables only time triggers
        /// </summary>        
        public static void AddTriggers(this ContainerBuilder containerBuilder)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddTriggers();
            containerBuilder.Populate(serviceCollection);
        }
    }
}
