using System;
using System.Threading.Tasks;
using Lykke.JobTriggers.Abstractions;

namespace Lykke.JobTriggers.Implementations
{
    public class EmptyNotifier : IPoisionQueueNotifier
    {
        public Task NotifyAsync(string message)
        {
            Console.WriteLine(message);
            return Task.CompletedTask;
        }
    }
}
