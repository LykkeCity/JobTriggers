using System;

namespace Lykke.JobTriggers.Triggers.Delay
{
	internal interface IDelayStrategy
	{
		TimeSpan GetNextDelay(bool executionSucceeded);
	}
}
