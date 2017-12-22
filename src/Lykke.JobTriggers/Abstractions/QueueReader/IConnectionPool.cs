using Lykke.SettingsReader;

namespace Lykke.JobTriggers.Abstractions.QueueReader
{
    public interface IConnectionPool
    {
        void AddConnection(string alias, IReloadingManager<string> connectionString);
        IReloadingManager<string> GetConnection(string alias);
        void AddDefaultConnection(IReloadingManager<string> connectionString);
        bool HasConnection(string alias);
    }
}
