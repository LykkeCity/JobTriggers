using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.JobTriggers.Triggers.Bindings;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.JobTriggers.Triggers
{
    public class TriggerHost
    {
        private readonly IServiceProvider _serviceProvider;

        private readonly List<ITriggerBinding> _bindings = new List<ITriggerBinding>();

        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private Assembly[] _assemblies;

        internal static bool UseQueueTriggers { get; set; }

        public TriggerHost(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task Start()
        {
            var assemblies = CollectAssemblies().ToList();

            _bindings.AddRange(new TriggerBindingCollector<TimerTriggerBinding>().CollectFromAssemblies(assemblies, _serviceProvider));
            if (UseQueueTriggers)
                _bindings.AddRange(new TriggerBindingCollector<QueueTriggerBinding>().CollectFromAssemblies(assemblies, _serviceProvider));

            var tasks = _bindings.Select(o => o.RunAsync(_cancellationTokenSource.Token)).ToArray();

            var logFactory = _serviceProvider.GetService<ILogFactory>();
            // TODO: ILog resolving should be removed, when legacy logging system will be removed from the Lykke.Common
            var log = logFactory?.CreateLog(this) ?? _serviceProvider.GetRequiredService<ILog>();

            await Task.WhenAll(tasks).ContinueWith(
                task => log.WriteInfoAsync("TriggerHost", "Start", "", "All bindings are finished")).Unwrap().ConfigureAwait(false);
        }

        public void ProvideAssembly(params Assembly[] assemblies)
        {
            _assemblies = assemblies;
        }

        private IEnumerable<Assembly> CollectAssemblies()
        {
            var list = new List<Assembly>() { Assembly.GetEntryAssembly() };
            if (_assemblies != null)
                list.AddRange(_assemblies);

            return list.GroupBy(x => x.FullName).Select(x => x.First());
        }

        public void Cancel()
        {
            _cancellationTokenSource.Cancel();
        }


    }
}
