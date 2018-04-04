﻿using System;

namespace Lykke.JobTriggers.Abstractions.QueueReader
{
    public interface IQueueReaderFactory
    {
        IQueueReader Create(string connection, string queueName);
        IQueueReader Create(string connection, string queueName, TimeSpan queueTimeout);
    }
}
