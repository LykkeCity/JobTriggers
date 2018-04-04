using System;
using AzureStorage.Queue;
using Lykke.JobTriggers.Abstractions.QueueReader;
using IQueueReader = Lykke.JobTriggers.Abstractions.QueueReader.IQueueReader;

namespace Lykke.JobTriggers.Implementations.QueueReader
{
    public class AzureQueueReaderFactory : IQueueReaderFactory
    {
        private readonly IConnectionPool _connectionPool;

        public AzureQueueReaderFactory(IConnectionPool connectionPool)
        {
            _connectionPool = connectionPool;
        }

        public IQueueReader Create(string connection, string queueName)
        {
            return new AzureQueueReader(AzureQueueExt.Create(_connectionPool.GetConnection(connection), queueName));
        }

        public IQueueReader Create(string connection, string queueName, TimeSpan queueTimeout)
        {
            return new AzureQueueReader(AzureQueueExt.Create(_connectionPool.GetConnection(connection), queueName, queueTimeout));
        }
    }
}
