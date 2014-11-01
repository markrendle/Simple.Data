using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Simple.Data.Extensions;

namespace Simple.Data.Ado
{
    using System.Threading.Tasks;
    using Schema;

    class AdoAdapterInserter
    {
        private static readonly IDictionary<string, object> NullDictionary = null;
        private readonly AdoAdapter _adapter;
        private readonly IDbConnection _connection;
        private readonly IDbTransaction _transaction;

        public AdoAdapterInserter(AdoAdapter adapter) : this(adapter, (IDbTransaction)null)
        {
        }

        public AdoAdapterInserter(AdoAdapter adapter, IDbConnection connection)
        {
            _adapter = adapter;
            _connection = connection;
        }

        public AdoAdapterInserter(AdoAdapter adapter, IDbTransaction transaction)
        {
            _adapter = adapter;
            _transaction = transaction;
            if (transaction != null) _connection = transaction.Connection;
        }

        public Task<IEnumerable<IDictionary<string, object>>> InsertMany(string tableName, CheckedEnumerable<IReadOnlyDictionary<string, object>> data, ErrorCallback onError, bool resultRequired)
        {
            if (data == null) throw new ArgumentNullException("data");
            var list = data.ToList();
            var table = _adapter.GetSchema().FindTable(tableName);
            foreach (var row in list)
            {
                CheckInsertablePropertiesAreAvailable(table, row);
            }

            var bulkInserter = _adapter.ProviderHelper.GetCustomProvider<IBulkInserter>(_adapter.ConnectionProvider) ?? new BulkInserter();
            return bulkInserter.Insert(_adapter, tableName, list, _transaction, onError, resultRequired);
        }

        public async Task<IDictionary<string, object>> Insert(string tableName, IEnumerable<KeyValuePair<string, object>> data, bool resultRequired)
        {
            var table = _adapter.GetSchema().FindTable(tableName);
            var dataArray = data.ToArray();
            CheckInsertablePropertiesAreAvailable(table, dataArray);

            var customInserter = _adapter.ProviderHelper.GetCustomProvider<ICustomInserter>(_adapter.ConnectionProvider);
            if (customInserter != null)
            {
                return await customInserter.Insert(_adapter, tableName, dataArray.ToDictionary(), _transaction, resultRequired);
            }

            var dataDictionary = dataArray.Where(kvp => table.HasColumn(kvp.Key) && table.FindColumn(kvp.Key).IsWriteable)
                                     .ToDictionary(kvp => table.FindColumn(kvp.Key), kvp => kvp.Value);

            string columnList = dataDictionary.Keys.Select(c => c.QuotedName).Aggregate((agg, next) => agg + "," + next);
            string valueList = dataDictionary.Keys.Select(s => "?").Aggregate((agg, next) => agg + "," + next);

            string insertSql = "insert into " + table.QualifiedName + " (" + columnList + ") values (" + valueList + ")";

            if (resultRequired)
            {
                var identityFunction = _adapter.GetIdentityFunction();
                if (!string.IsNullOrWhiteSpace(identityFunction))
                {
                    var identityColumn = table.Columns.FirstOrDefault(col => col.IsIdentity);

                    if (identityColumn != null)
                    {
                        var selectSql = "select * from " + table.QualifiedName + " where " + identityColumn.QuotedName +
                                        " = " + identityFunction;
                        if (_adapter.ProviderSupportsCompoundStatements)
                        {
                            insertSql += "; " + selectSql;
                            return await ExecuteSingletonQuery(insertSql, dataDictionary.Keys, dataDictionary.Values);
                        }
                        return await ExecuteSingletonQuery(insertSql, selectSql, dataDictionary.Keys,
                                                     dataDictionary.Values);
                    }
                }
            }

            Execute(insertSql, dataDictionary.Keys, dataDictionary.Values);
            return null;
        }

        private void CheckInsertablePropertiesAreAvailable(Table table, IEnumerable<KeyValuePair<string, object>> data)
        {
            data = data.Where(kvp => table.HasColumn(kvp.Key));

            if (!data.Any())
            {
                throw new SimpleDataException(string.Format("No properties were found which could be mapped to table '{0}'.", table.ActualName));
            }
        }

        internal Task<IDictionary<string, object>> ExecuteSingletonQuery(string sql, IEnumerable<Column> columns, IEnumerable<Object> values)
        {
            if (_transaction != null)
            {
                var command = new CommandHelper(_adapter).CreateInsert(_transaction.Connection, sql, columns, values.ToArray());
                command.Transaction = _transaction;
                return TryExecuteSingletonQuery(command);
            }

            var connection = _connection ?? _adapter.CreateConnection();
            using (connection.MaybeDisposable())
            {
                using (var command = new CommandHelper(_adapter).CreateInsert(connection, sql, columns, values.ToArray()))
                {
                    connection.OpenIfClosed();
                    return TryExecuteSingletonQuery(command);
                }
            }
        }

        internal Task<IDictionary<string, object>> ExecuteSingletonQuery(string insertSql, string selectSql, IEnumerable<Column> columns, IEnumerable<Object> values)
        {
            if (_transaction != null)
            {
                var command = new CommandHelper(_adapter).CreateInsert(_transaction.Connection, insertSql, columns, values.ToArray());
                command.Transaction = _transaction;
                command.TryExecuteNonQuery();
                command.CommandText = selectSql;
                command.Parameters.Clear();
                return TryExecuteSingletonQuery(command);
            }

            var connection = _connection ?? _adapter.CreateConnection();
            using (connection.MaybeDisposable())
            {
                using (var command = new CommandHelper(_adapter).CreateInsert(connection, insertSql, columns, values.ToArray()))
                {
                    connection.OpenIfClosed();
                    command.TryExecuteNonQuery();
                    command.CommandText = selectSql;
                    command.Parameters.Clear();
                    return TryExecuteSingletonQuery(command);
                }
            }
        }

        private async Task<IDictionary<string, object>> TryExecuteSingletonQuery(IDbCommand command)
        {
            using (var reader = await _adapter.CommandExecutor.ExecuteReader(command))
            {
                if (reader.Read())
                {
                    return reader.ToDictionary();
                }
            }

            return null;
        }

        internal int Execute(string sql, IEnumerable<Column> columns, IEnumerable<Object> values)
        {
            if (_transaction != null)
            {
                var command = new CommandHelper(_adapter).CreateInsert(_transaction.Connection, sql, columns, values.ToArray());
                command.Transaction = _transaction;
                return command.TryExecuteNonQuery();
            }
            var connection = _connection ?? _adapter.CreateConnection();
            using (connection.MaybeDisposable())
            {
                using (var command = new CommandHelper(_adapter).CreateInsert(connection, sql, columns, values.ToArray()))
                {
                    connection.OpenIfClosed();
                    return command.TryExecuteNonQuery();
                }
            }
        }
    }
}
