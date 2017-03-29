using System;
using System.Globalization;

namespace Lykke.JobTriggers.Triggers.Attributes
{
    public class TimerTriggerAttribute : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="period">Period's format is d.hh:mm:ss</param>
        public TimerTriggerAttribute(string period)
        {
            TimeSpan value;
            if (!TimeSpan.TryParseExact(period, "c", CultureInfo.InvariantCulture, out value))
                throw new ArgumentException("Can't parse to timespan. Expected format is d.hh:mm:ss", nameof(period));
            Period = value;
        }

        public TimeSpan Period { get; }
    }
}
