using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.Linq;
using System.Text;
using Simple.Data.Ado;
using Simple.Data.Ado.Schema;

namespace Simple.Data.SqlServer
{
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.Threading.Tasks;

    [Export(typeof(ICustomInserter))]
    public class SqlCustomInserter : ICustomInserter
    {
        public async Task<IDictionary<string, object>> Insert(AdoAdapter adapter, string tableName, IDictionary<string, object> data, IDbTransaction transaction = null, bool resultRequired = false)
        {
            var table = adapter.GetSchema().FindTable(tableName);
            var dataDictionary = BuildDataDictionary(adapter, data, table);

            string columnList = dataDictionary.Keys.Select(c => c.QuotedName).Aggregate((agg, next) => agg + "," + next);
            string valueList = dataDictionary.Keys.Select(s => "?").Aggregate((agg, next) => agg + "," + next);

            var insertSql = new StringBuilder();
            bool identityInsert = adapter.AdoOptions != null && adapter.AdoOptions.IdentityInsert;
            if (identityInsert)
            {
                insertSql.AppendFormat("SET IDENTITY_INSERT {0} ON; ", table.QualifiedName);
            }
            insertSql.AppendFormat("INSERT INTO {0} ({1})", table.QualifiedName, columnList);
            insertSql.AppendFormat(" VALUES ({0})", valueList);
            
            if (identityInsert)
            {
                insertSql.AppendFormat("; SET IDENTITY_INSERT {0} OFF; ", table.QualifiedName);
            }

            if (resultRequired)
            {
                var identityColumn = table.Columns.FirstOrDefault(c => c.IsIdentity);
                if (identityColumn != null)
                {
                    insertSql.AppendFormat(" SELECT * FROM {0} WHERE {1} = SCOPE_IDENTITY()", table.QualifiedName,
                                           identityColumn.QuotedName);
                    return await ExecuteSingletonQuery(adapter, insertSql.ToString(), dataDictionary.Keys,
                                                 dataDictionary.Values, transaction);
                }
            }
            Execute(adapter, insertSql.ToString(), dataDictionary.Keys, dataDictionary.Values, transaction);
            return null;
        }

        private static Dictionary<Column, object> BuildDataDictionary(AdoAdapter adapter, IDictionary<string, object> data, Table table)
        {
            Func<string, bool> columnFilter;
            if (adapter.AdoOptions != null && adapter.AdoOptions.IdentityInsert)
            {
                columnFilter =
                    key =>
                        {
                            Column column;
                            if (table.TryFindColumn(key, out column))
                            {
                                return column.IsWriteable || column.IsIdentity;
                            }
                            return false;
                        };
            }
            else
            {
                columnFilter = key => table.HasColumn(key) && table.FindColumn(key).IsWriteable;
            }
            var dataDictionary = data.Where(kvp => columnFilter(kvp.Key))
                .ToDictionary(kvp => table.FindColumn(kvp.Key), kvp => kvp.Value);
            return dataDictionary;
        }

        internal async Task<IDictionary<string, object>> ExecuteSingletonQuery(AdoAdapter adapter, string sql, IEnumerable<Column> columns, IEnumerable<Object> values, IDbTransaction transaction)
        {
            if (transaction != null)
            {
                var command = new CommandHelper(adapter).CreateInsert(transaction.Connection, sql, columns, values.ToArray());
                command.Transaction = transaction;
                return await TryExecuteSingletonQuery(command);
            }

            var connection = adapter.CreateConnection();
            using (connection.MaybeDisposable())
            {
                using (var command = new CommandHelper(adapter).CreateInsert(connection, sql, columns, values.ToArray()))
                {
                    connection.OpenIfClosed();
                    return await TryExecuteSingletonQuery(command);
                }
            }
        }

        private static async Task<IDictionary<string, object>> TryExecuteSingletonQuery(IDbCommand command)
        {
            IDictionary<string, object> result = null;
            bool close = false;
            var sqlCommand = ((SqlCommand)command);
            if (sqlCommand.Connection.State == ConnectionState.Closed)
            {
                await sqlCommand.Connection.OpenAsync();
                close = true;
            }
            try
            {
                if (sqlCommand.Connection.State == ConnectionState.Closed)
                {
                    Debugger.Break();
                }
                var reader = await sqlCommand.ExecuteReaderAsync();
                if (reader.Read())
                {
                    result = reader.ToDictionary();
                }
            }
            catch (InvalidOperationException)
            {
                Debugger.Break();
            }
            finally 
            {
                if (close)
                {
                    sqlCommand.Connection.Close();
                }
            }
            return result;
        }

        internal int Execute(AdoAdapter adapter, string sql, IEnumerable<Column> columns, IEnumerable<Object> values, IDbTransaction transaction)
        {
            if (transaction != null)
            {
                var command = new CommandHelper(adapter).CreateInsert(transaction.Connection, sql, columns, values.ToArray());
                command.Transaction = transaction;
                return command.TryExecuteNonQuery();
            }
            var connection = adapter.CreateConnection();
            using (connection.MaybeDisposable())
            {
                using (var command = new CommandHelper(adapter).CreateInsert(connection, sql, columns, values.ToArray()))
                {
                    connection.OpenIfClosed();
                    return command.TryExecuteNonQuery();
                }
            }
        }
    }
}