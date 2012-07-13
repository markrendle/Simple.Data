namespace Simple.Data.Ado
{
    using System.Dynamic;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Linq;

    class AdoAdapterFinder
    {
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, CommandTemplate>> _commandCaches =
            new ConcurrentDictionary<string, ConcurrentDictionary<string, CommandTemplate>>();
        private readonly AdoAdapter _adapter;
        private readonly IDbTransaction _transaction;
        private readonly IDbConnection _connection;

        public AdoAdapterFinder(AdoAdapter adapter) : this(adapter, (IDbTransaction)null)
        {
        }

        public AdoAdapterFinder(AdoAdapter adapter, IDbConnection connection)
        {
            _adapter = adapter;
            _connection = connection;
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

        public IDictionary<string, object> FindOne(string tableName, SimpleExpression criteria)
        {
            if (criteria == null) return FindAll(_adapter.GetSchema().BuildObjectName(tableName)).FirstOrDefault();
            var commandTemplate = GetCommandTemplate(tableName, criteria);
            return ExecuteSingletonQuery(commandTemplate, criteria.GetValues());
        }

        public Func<object[],IDictionary<string,object>> CreateFindOneDelegate(string tableName, SimpleExpression criteria)
        {
            if (criteria == null)
            {
                return _ => FindAll(_adapter.GetSchema().BuildObjectName(tableName)).FirstOrDefault();
            }
            var commandBuilder = new FindHelper(_adapter.GetSchema())
                .GetFindByCommand(_adapter.GetSchema().BuildObjectName(tableName), criteria);

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
            else
            {
                return args => ExecuteSingletonQuery(commandTemplate, args);
            }
        }

        private IDictionary<string, object> ExecuteSingletonQuery(IDbCommand command, object[] parameterValues, IDictionary<string,int> index)
        {
            for (int i = 0; i < command.Parameters.Count; i++)
            {
                ((IDbDataParameter) command.Parameters[i]).Value = FixObjectType(parameterValues[i]);
            }
            command.Connection = _adapter.CreateConnection();
            return TryExecuteSingletonQuery(command.Connection, command, index);
        }

        public IEnumerable<IDictionary<string, object>> Find(string tableName, SimpleExpression criteria)
        {
            if (criteria == null) return FindAll(_adapter.GetSchema().BuildObjectName(tableName));
            var commandTemplate = GetCommandTemplate(tableName, criteria);
            return ExecuteQuery(commandTemplate, criteria.GetValues());
        }

        private CommandTemplate GetCommandTemplate(string tableName, SimpleExpression criteria)
        {
            var tableCommandCache = _commandCaches.GetOrAdd(tableName,
                                                            _ => new ConcurrentDictionary<string, CommandTemplate>());

            var hash = new ExpressionHasher().Format(criteria);
            return tableCommandCache.GetOrAdd(hash,
                                              _ =>
                                              new FindHelper(_adapter.GetSchema())
                                                  .GetFindByCommand(_adapter.GetSchema().BuildObjectName(tableName), criteria)
                                                  .GetCommandTemplate(_adapter.GetSchema().FindTable(_adapter.GetSchema().BuildObjectName(tableName))));
        }

        private IEnumerable<IDictionary<string, object>> FindAll(ObjectName tableName)
        {
            return ExecuteQuery("select * from " + _adapter.GetSchema().FindTable(tableName).QualifiedName);
        }

        private IEnumerable<IDictionary<string, object>> ExecuteQuery(CommandTemplate commandTemplate, IEnumerable<object> parameterValues)
        {
            var connection = _connection ?? _adapter.CreateConnection();
            var command = commandTemplate.GetDbCommand(_adapter, connection, parameterValues);
            command.Transaction = _transaction;
            return TryExecuteQuery(connection, command, commandTemplate.Index);
        }

        private IDictionary<string, object> ExecuteSingletonQuery(CommandTemplate commandTemplate, IEnumerable<object> parameterValues)
        {
            var connection = _connection ?? _adapter.CreateConnection();
            var command = commandTemplate.GetDbCommand(_adapter, connection, parameterValues);
            command.Transaction = _transaction;
            return TryExecuteSingletonQuery(connection, command, commandTemplate.Index);
        }

        private IEnumerable<IDictionary<string, object>> ExecuteQuery(string sql, params object[] values)
        {
            var connection = _connection ?? _adapter.CreateConnection();
            var command = new CommandHelper(_adapter).Create(connection, sql, values);
            command.Transaction = _transaction;
            return TryExecuteQuery(connection, command);
        }

        private IEnumerable<IDictionary<string, object>> TryExecuteQuery(IDbConnection connection, IDbCommand command)
        {
            try
            {
                return command.ToEnumerable(ConnectionCreator);
            }
            catch (DbException ex)
            {
                throw new AdoAdapterException(ex.Message, command);
            }
        }

        private IEnumerable<IDictionary<string, object>> TryExecuteQuery(IDbConnection connection, IDbCommand command, IDictionary<string, int> index)
        {
            try
            {
                return command.ToEnumerable(ConnectionCreator);
            }
            catch (DbException ex)
            {
                throw new AdoAdapterException(ex.Message, command);
            }
        }

        private Func<IDbConnection> ConnectionCreator
        {
            get
            {
                if (_transaction != null)
                {
                    return () => _transaction.Connection;
                }
                return _adapter.CreateConnection;
            }
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

        private static IDisposable DisposeWrap(IDbConnection connection)
        {
            if (connection.State == ConnectionState.Open)
            {
                return ActionDisposable.NoOp;
            }

            return new ActionDisposable(connection.Dispose);
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
    }
}
