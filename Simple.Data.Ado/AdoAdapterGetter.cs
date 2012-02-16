using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Dynamic;
using System.Linq;

namespace Simple.Data.Ado
{
    class AdoAdapterGetter
    {
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, CommandTemplate>> _commandCaches =
            new ConcurrentDictionary<string, ConcurrentDictionary<string, CommandTemplate>>();
        private readonly AdoAdapter _adapter;
        private readonly IDbTransaction _transaction;
        private readonly IDbConnection _connection;

        public AdoAdapterGetter(AdoAdapter adapter) : this(adapter, null)
        {
        }

        public AdoAdapterGetter(AdoAdapter adapter, IDbTransaction transaction)
        {
            if (adapter == null) throw new ArgumentNullException("adapter");
            _adapter = adapter;

            if (transaction != null)
            {
                _transaction = transaction;
                _connection = transaction.Connection;
            }
        }

        public Func<object[],IDictionary<string,object>> CreateGetDelegate(string tableName, params object[] keyValues)
        {
            var primaryKey = _adapter.GetSchema().FindTable(tableName).PrimaryKey;
            if (primaryKey == null) throw new InvalidOperationException("Table has no primary key.");
            if (primaryKey.Length != keyValues.Length) throw new ArgumentException("Incorrect number of values for key.");


            var commandBuilder = new GetHelper(_adapter.GetSchema()).GetCommand(_adapter.GetSchema().FindTable(tableName), keyValues);

            var command = commandBuilder.GetCommand(_adapter.CreateConnection());
            command = _adapter.CommandOptimizer.OptimizeFindOne(command);

            var commandTemplate =
                commandBuilder.GetCommandTemplate(
                    _adapter.GetSchema().FindTable(_adapter.GetSchema().BuildObjectName(tableName)));

            var cloneable = command as ICloneable;
            if (cloneable != null)
            {
                return args => ExecuteSingletonQuery((IDbCommand)cloneable.Clone(), args, commandTemplate.Index);
            }
            return args => ExecuteSingletonQuery(commandTemplate, args);
        }

        private IDictionary<string, object> ExecuteSingletonQuery(IDbCommand command, object[] parameterValues, IDictionary<string,int> index)
        {
            for (int i = 0; i < command.Parameters.Count; i++)
            {
                ((IDbDataParameter) command.Parameters[i]).Value = FixObjectType(parameterValues[i]);
            }
            command.Connection = _connection ?? _adapter.CreateConnection();
            command.Transaction = _transaction;
            return TryExecuteSingletonQuery(command.Connection, command, index);
        }

        private IDictionary<string, object> ExecuteSingletonQuery(CommandTemplate commandTemplate, IEnumerable<object> parameterValues)
        {
            var connection = _connection ?? _adapter.CreateConnection();
            var command = commandTemplate.GetDbCommand(connection, parameterValues);
            command.Transaction = _transaction;
            return TryExecuteSingletonQuery(connection, command, commandTemplate.Index);
        }

        private static IDictionary<string, object> TryExecuteSingletonQuery(IDbConnection connection, IDbCommand command, IDictionary<string, int> index)
        {
            command.WriteTrace();
            using (connection.MaybeDisposable())
            using (command)
            {
                try
                {
                    connection.OpenIfClosed();
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return reader.ToDictionary(index);
                        }
                    }
                }
                catch (DbException ex)
                {
                    throw new AdoAdapterException(ex.Message, command);
                }
            }
            return null;
        }

        private static object FixObjectType(object value)
        {
            if (value == null) return DBNull.Value;
            if (TypeHelper.IsKnownType(value.GetType())) return value;
            var dynamicObject = value as DynamicObject;
            if (dynamicObject != null)
            {
                return dynamicObject.ToString();
            }
            return value;
        }

        public IDictionary<string, object> Get(string tableName, object[] parameterValues)
        {
            var primaryKey = _adapter.GetSchema().FindTable(tableName).PrimaryKey;
            if (primaryKey == null) throw new InvalidOperationException("Table has no primary key.");
            if (primaryKey.Length != parameterValues.Length) throw new ArgumentException("Incorrect number of values for key.");


            var commandBuilder = new GetHelper(_adapter.GetSchema()).GetCommand(_adapter.GetSchema().FindTable(tableName), parameterValues);

            var command = commandBuilder.GetCommand(_adapter.CreateConnection());
            command = _adapter.CommandOptimizer.OptimizeFindOne(command);

            var commandTemplate =
                commandBuilder.GetCommandTemplate(
                    _adapter.GetSchema().FindTable(_adapter.GetSchema().BuildObjectName(tableName)));

            return ExecuteSingletonQuery(command, parameterValues, commandTemplate.Index);
        }
    }
}