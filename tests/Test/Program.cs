using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Autofac.Features.ResolveAnything;
using Common.Log;
using Lykke.JobTriggers.Extenstions;
using Lykke.JobTriggers.Triggers;
using Lykke.SettingsReader;

namespace Test
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ContainerBuilder builder = new ContainerBuilder();

            ILog log = new LogToConsole();

            builder.RegisterInstance(log);

            builder.AddTriggers(pool =>
            {
                pool.AddDefaultConnection(new FakeReloadingManager("UseDevelopmentStorage=true"));
            });

            builder.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource());
            var container = builder.Build();

            var host = new TriggerHost(new AutofacServiceProvider(container));
            var task = host.Start();

            Console.ReadKey();
            host.Cancel();
            task.Wait();
            Console.ReadKey();
        }

        public class FakeReloadingManager : IReloadingManager<string>
        {
            private readonly string _value;

            public FakeReloadingManager(string value)
            {
                _value = value;
            }

            public Task<string> Reload() => Task.FromResult(_value);
            public bool HasLoaded => true;
            public string CurrentValue => _value;
        }
    }
}
