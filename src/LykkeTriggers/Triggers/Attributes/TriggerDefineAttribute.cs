using System;

namespace LykkeTriggers.Triggers.Attributes
{
	public class TriggerDefineAttribute : Attribute
	{
		public Type Type { get; }

		public TriggerDefineAttribute(Type type)
		{
			Type = type;
		}
	}
}
