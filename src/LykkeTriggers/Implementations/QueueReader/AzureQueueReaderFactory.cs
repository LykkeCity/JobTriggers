
using AzureStorage.Queue;
using LykkeTriggers.Abstractions.QueueReader;
using IQueueReader = LykkeTriggers.Abstractions.QueueReader.IQueueReader;

namespace LykkeTriggers.Implementations.QueueReader
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
            return new AzureQueueReader(new AzureQueueExt(_connectionPool.GetConnection(connection), queueName));
        }
    }
}
