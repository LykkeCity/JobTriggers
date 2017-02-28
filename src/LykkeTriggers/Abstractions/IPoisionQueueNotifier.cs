using System.Threading.Tasks;

namespace LykkeTriggers.Abstractions
{
    public interface IPoisionQueueNotifier
    {     
        Task NotifyAsync(string message);     
    }
}
