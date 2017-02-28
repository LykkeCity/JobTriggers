using System.Threading.Tasks;

namespace Lykke.JobTriggers.Abstractions
{
    public interface IPoisionQueueNotifier
    {     
        Task NotifyAsync(string message);     
    }
}
