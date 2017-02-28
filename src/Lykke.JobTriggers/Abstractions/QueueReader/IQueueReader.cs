using System;
using System.Threading.Tasks;

namespace Lykke.JobTriggers.Abstractions.QueueReader
{
	public interface IQueueMessage
	{
		object Value(Type type);
		string AsString { get; }
		int DequeueCount { get; }
	    DateTimeOffset InsertionTime { get; }
	}

	public interface IQueueReader
	{
		Task<IQueueMessage> GetMessageAsync();
		Task AddMessageAsync(string message);
		Task FinishMessageAsync(IQueueMessage msg);
		Task ReleaseMessageAsync(IQueueMessage msg);
	    Task<int> Count();
	}
}
