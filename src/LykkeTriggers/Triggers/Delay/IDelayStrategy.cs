using System;

namespace LykkeTriggers.Triggers.Delay
{
	internal interface IDelayStrategy
	{
		TimeSpan GetNextDelay(bool executionSucceeded);
	}
}
