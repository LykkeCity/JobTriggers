using System;
using Lykke.JobTriggers.Implementations.QueueReader;

namespace Lykke.JobTriggers.Triggers.Attributes
{
    public class QueueTriggerAttribute : Attribute
    {
        public string Queue { get; }
        public int MaxPollingIntervalMs { get; }
        public bool Notify { get; }
        public string Connection { get; set; }
        public int MaxDequeueCount { get; }
        public int TimeoutInSeconds { get; }

        public QueueTriggerAttribute(
            string queue,
            int maxPollingIntervalMs = -1,
            bool notify = false,
            string connection = ConnectionPool.DefaultConnection,
            int maxDequeueCount = 5,
            int timeoutInSeconds = 60)
        {
            Queue = queue;
            MaxPollingIntervalMs = maxPollingIntervalMs;
            Notify = notify;
            Connection = connection;
            MaxDequeueCount = maxDequeueCount;
            TimeoutInSeconds = timeoutInSeconds;
        }
    }
}
