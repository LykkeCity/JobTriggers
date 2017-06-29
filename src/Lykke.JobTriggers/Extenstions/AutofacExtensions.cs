using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Common.Log;
using Lykke.JobTriggers.Abstractions;
using Lykke.JobTriggers.Abstractions.QueueReader;
using Lykke.JobTriggers.Autofac;
using Lykke.JobTriggers.Implementations;
using Lykke.JobTriggers.Implementations.QueueReader;
using Lykke.JobTriggers.Triggers;
using Lykke.JobTriggers.Triggers.Bindings;

namespace Lykke.JobTriggers.Extenstions
{
    public static class AutofacExtensions
    {
        /// <summary>
        /// Method enables time and queue triggers
        /// </summary>     
        /// <param name="containerBuilder"></param>   
        /// <param name="connectionPoolSetup">Used to add connections</param>
        public static void AddTriggers(this ContainerBuilder containerBuilder, Action<IConnectionPool> connectionPoolSetup)
        {
            containerBuilder.AddTriggers();

            containerBuilder.RegisterType<EmptyNotifier>()
                            .As<IPoisionQueueNotifier>()
                            .IfNotRegistered(typeof(IPoisionQueueNotifier));

            var connectionPool = new ConnectionPool();

            connectionPoolSetup(connectionPool);

            if (!connectionPool.HasConnection(ConnectionPool.DefaultConnection))
                throw new Exception("Connection pool should have default connection string");

            containerBuilder.Register(x => new AzureQueueReaderFactory(connectionPool))
                            .As<IQueueReaderFactory>()
                            .SingleInstance();

            TriggerHost.UseQueueTriggers = true;
        }

        /// <summary>
        /// Method enables only time triggers
        /// </summary>
        /// <param name="containerBuilder"></param>
        public static void AddTriggers(this ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterType<LogToConsole>()
                            .As<ILog>()
                            .IfNotRegistered(typeof(ILog));

            containerBuilder.RegisterType<QueueTriggerBinding>();
            containerBuilder.RegisterType<TimerTriggerBinding>();

            containerBuilder.RegisterSource(new JobFunctionsNotAlreadyRegisteredSource());
        }
    }
}
