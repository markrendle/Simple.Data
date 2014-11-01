namespace Simple.Data.Ado
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Threading;
    using System.Threading.Tasks;

    public interface ICommandExecutor
    {
        Task OpenIfClosed(IDbConnection connection);
        Task<IDataReader> ExecuteReader(IDbCommand command);
        Task<int> ExecuteNonQuery(IDbCommand command);
        Task<object> ExecuteScalar(IDbCommand command);
        Task<bool> NextResult(IDataReader reader);
    }

    public class DefaultCommandExecutor : ICommandExecutor
    {
        public Task OpenIfClosed(IDbConnection connection)
        {
            return Task.Run(() => connection.OpenIfClosed());
        }

        public Task<IDataReader> ExecuteReader(IDbCommand command)
        {
            return Task.Run(() => command.ExecuteReader());
        }

        public Task<int> ExecuteNonQuery(IDbCommand command)
        {
            return Task.Run(() => command.ExecuteNonQuery());
        }

        public Task<object> ExecuteScalar(IDbCommand command)
        {
            return Task.Run(() => command.ExecuteScalar());
        }

        public Task<bool> NextResult(IDataReader reader)
        {
            return Task.Run(() => reader.NextResult());
        }
    }
}