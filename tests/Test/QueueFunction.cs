using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.JobTriggers.Triggers.Attributes;

namespace Test
{
    public class QueueFunction
    {
        [QueueTrigger("test-queue", 1000)]
        public void OnMessage(string message)
        {
            Console.WriteLine("QueueTrigger: "+message);
        }
    }
}
