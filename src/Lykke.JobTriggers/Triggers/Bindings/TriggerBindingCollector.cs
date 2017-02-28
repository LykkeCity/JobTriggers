using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Lykke.JobTriggers.Triggers.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.JobTriggers.Triggers.Bindings
{
    public class TriggerBindingCollector<T> where T : ITriggerBinding
    {       
        public List<T> CollectFromAssemblies(List<Assembly> assemblies, IServiceProvider serviceProvider)
        {
            var defineAttribute = typeof(T).GetTypeInfo().GetCustomAttribute<TriggerDefineAttribute>();
            if (defineAttribute == null)
                throw new Exception("Type T must have TriggerDefineAttribute");

            return assemblies.SelectMany(x=>x.GetTypes()).SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(
                m => m.GetCustomAttribute(defineAttribute.Type, false) != null))
                .Select(m =>
                {
                    var binding = serviceProvider.GetService<T>();
                    binding.InitBinding(serviceProvider, m);
                    return binding;
                }).ToList();
        }
    }
}
