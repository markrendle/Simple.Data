using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Simple.Data.Ado
{
    class AdoAdapterFinder
    {
        private readonly AdoAdapter _adapter;
        private readonly IDbTransaction _transaction;
        private readonly IDbConnection _connection;

        public AdoAdapterFinder(AdoAdapter adapter) : this(adapter, null)
        {
        }

        public AdoAdapterFinder(AdoAdapter adapter, IDbTransaction transaction)
        {
            if (adapter == null) throw new ArgumentNullException("adapter");
            _adapter = adapter;

            if (transaction != null)
            {
                _transaction = transaction;
                _connection = transaction.Connection;
            }
        }

        public IEnumerable<IDictionary<string, object>> Find(string tableName, SimpleExpression criteria)
        {
            if (criteria == null) return FindAll(ObjectName.Parse(tableName));

            var commandBuilder = new FindHelper(_adapter.GetSchema()).GetFindByCommand(ObjectName.Parse(tableName), criteria);
            return ExecuteQuery(commandBuilder);
        }

        private IEnumerable<IDictionary<string, object>> FindAll(ObjectName tableName)
        {
            return ExecuteQuery("select * from " + _adapter.GetSchema().FindTable(tableName).ActualName);
        }

        private IEnumerable<IDictionary<string, object>> ExecuteQuery(ICommandBuilder commandBuilder)
        {
            var connection = _connection ?? _adapter.CreateConnection();
            var command = commandBuilder.GetCommand(connection);
            command.Transaction = _transaction;
            return TryExecuteQuery(command);
        }

        private IEnumerable<IDictionary<string, object>> ExecuteQuery(string sql, params object[] values)
        {
            var connection = _connection ?? _adapter.CreateConnection();
            var command = new CommandHelper(_adapter.SchemaProvider).Create(connection, sql, values);
            command.Transaction = _transaction;
            return TryExecuteQuery(command);
        }

        private static IEnumerable<IDictionary<string, object>> TryExecuteQuery(IDbCommand command)
        {
            try
            {
                return command.ToBufferedEnumerable();
            }
            catch (DbException ex)
            {
                throw new AdoAdapterException(ex.Message, command);
            }
        }
    }
}
