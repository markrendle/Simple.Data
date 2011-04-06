using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Simple.Data.Ado
{
    class AdoAdapterFinder
    {
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, CommandTemplate>> _commandCaches =
            new ConcurrentDictionary<string, ConcurrentDictionary<string, CommandTemplate>>();
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

            var tableCommandCache = _commandCaches.GetOrAdd(tableName,
                                                           _ => new ConcurrentDictionary<string, CommandTemplate>());

            var hash = new ExpressionHasher().Format(criteria);
            var commandTemplate = tableCommandCache.GetOrAdd(hash,
                                                            _ =>
                                                            new FindHelper(_adapter.GetSchema())
                                                                .GetFindByCommand(ObjectName.Parse(tableName), criteria)
                                                                .GetCommandTemplate());
            return ExecuteQuery(commandTemplate, criteria.GetValues());
        }

        private IEnumerable<IDictionary<string, object>> FindAll(ObjectName tableName)
        {
            return ExecuteQuery("select * from " + _adapter.GetSchema().FindTable(tableName).QualifiedName);
        }

        private IEnumerable<IDictionary<string, object>> ExecuteQuery(ICommandBuilder commandBuilder)
        {
            var connection = _connection ?? _adapter.CreateConnection();
            var command = commandBuilder.GetCommand(connection);
            command.Transaction = _transaction;
            return TryExecuteQuery(connection, command);
        }

        private IEnumerable<IDictionary<string, object>> ExecuteQuery(CommandTemplate commandTemplate, IEnumerable<object> parameterValues)
        {
            var connection = _connection ?? _adapter.CreateConnection();
            var command = commandTemplate.GetDbCommand(connection, parameterValues);
            command.Transaction = _transaction;
            return TryExecuteQuery(connection, command);
        }

        private IEnumerable<IDictionary<string, object>> ExecuteQuery(string sql, params object[] values)
        {
            var connection = _connection ?? _adapter.CreateConnection();
            var command = new CommandHelper(_adapter.SchemaProvider).Create(connection, sql, values);
            command.Transaction = _transaction;
            return TryExecuteQuery(connection, command);
        }

        private static IEnumerable<IDictionary<string, object>> TryExecuteQuery(IDbConnection connection, IDbCommand command)
        {
            try
            {
                return command.ToBufferedEnumerable(connection);
            }
            catch (DbException ex)
            {
                throw new AdoAdapterException(ex.Message, command);
            }
        }
    }
}
