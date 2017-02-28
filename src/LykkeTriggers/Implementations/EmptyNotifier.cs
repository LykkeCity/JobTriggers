using System;
using System.Threading.Tasks;
using AzureStorage.Queue;
using LykkeTriggers.Abstractions;
using Newtonsoft.Json;

namespace LykkeTriggers.Implementations
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
