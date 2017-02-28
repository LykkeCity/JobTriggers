using System;

namespace LykkeTriggers.Abstractions.QueueReader
{
    public interface IConnectionPool
    {
        void AddConnection(string alias, string connectionString);
        string GetConnection(string alias);
        void AddDefaultConnection(string connectionString);
        bool HasConnection(string alias);
    }
}
