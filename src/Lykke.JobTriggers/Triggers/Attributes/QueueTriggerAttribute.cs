using System;
using Lykke.JobTriggers.Implementations.QueueReader;

namespace Lykke.JobTriggers.Triggers.Attributes
{
    public class QueueTriggerAttribute : Attribute
    {
        public string Queue { get; }
        public TimeSpan Timeout { get; }
        public int MaxPollingIntervalMs { get; }
        public bool Notify { get; }
        public string Connection { get; set; }
        public int MaxDequeueCount { get; }

        public QueueTriggerAttribute(
            string queue,
            int maxPollingIntervalMs = -1,
            bool notify = false,
            string connection = ConnectionPool.DefaultConnection,
            int maxDequeueCount = 5)
            : this(
                  queue,
                  TimeSpan.FromMinutes(1),
                  maxPollingIntervalMs,
                  notify,
                  connection,
                  maxDequeueCount)
        {
        }

        public QueueTriggerAttribute(
            string queue,
            TimeSpan timeout,
            int maxPollingIntervalMs = -1,
            bool notify = false,
            string connection = ConnectionPool.DefaultConnection,
            int maxDequeueCount = 5)
        {
            Queue = queue;
            Timeout = timeout;
            MaxPollingIntervalMs = maxPollingIntervalMs;
            Notify = notify;
            Connection = connection;
            MaxDequeueCount = maxDequeueCount;
        }
    }
}
