namespace LykkeTriggers.Abstractions.QueueReader
{
    public interface IQueueReaderFactory
    {
	    IQueueReader Create(string connection, string queueName);
    }
}
