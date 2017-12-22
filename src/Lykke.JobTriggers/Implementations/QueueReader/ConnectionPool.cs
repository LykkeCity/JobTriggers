using System;
using System.Collections.Generic;
using Lykke.JobTriggers.Abstractions.QueueReader;
using Lykke.SettingsReader;

namespace Lykke.JobTriggers.Implementations.QueueReader
{
    public class ConnectionPool : IConnectionPool
    {
        public const string DefaultConnection = "default";

        private readonly Dictionary<string, IReloadingManager<string>> _connections = new Dictionary<string, IReloadingManager<string>>();

        public void AddConnection(string alias, IReloadingManager<string> connectionString)
        {
            _connections[alias] = connectionString;
        }

        public IReloadingManager<string> GetConnection(string alias)
        {
            if (!_connections.ContainsKey(alias))
                throw new Exception($"Connection alias '{alias}' is not registered");
            return _connections[alias];
        }

        public void AddDefaultConnection(IReloadingManager<string> connectionString)
        {
            AddConnection(DefaultConnection, connectionString);
        }

        public bool HasConnection(string alias)
        {
            return _connections.ContainsKey(alias);
        }
    }
}
