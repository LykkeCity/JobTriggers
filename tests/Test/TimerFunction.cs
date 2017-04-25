using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.JobTriggers.Triggers.Attributes;

namespace Test
{
    public class TimerFunction
    {
        [TimerTrigger("00:00:01")]
        public void OneSecondFunction()
        {
            Console.WriteLine("OneSecondFunction: " + DateTime.UtcNow);            
        }
    }
}
