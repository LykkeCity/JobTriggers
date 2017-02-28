using System;
using System.Collections.Generic;
using LykkeTriggers.Abstractions.QueueReader;

namespace LykkeTriggers.Implementations.QueueReader
{
    public class ConnectionPool : IConnectionPool
    {
        public const string DefaultConnection = "default";

        private readonly Dictionary<string, string> _connections = new Dictionary<string, string>();

        public void AddConnection(string alias, string connectionString)
        {
            _connections[alias] = connectionString;
        }

        public string GetConnection(string alias)
        {
            if (!_connections.ContainsKey(alias))
                throw new Exception($"Connection alias '{alias}' is not registered");
            return _connections[alias];
        }

        public void AddDefaultConnection(string connectionString)
        {
            AddConnection(DefaultConnection, connectionString);
        }

        public bool HasConnection(string alias)
        {
            return _connections.ContainsKey(alias);
        }
    }
}
