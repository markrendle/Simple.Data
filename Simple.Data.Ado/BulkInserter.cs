using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.Ado
{
    using System.Data;
    using System.Data.Common;
    using Schema;

    class BulkInserter : IBulkInserter
    {
        public IEnumerable<IDictionary<string, object>> Insert(AdoAdapter adapter, string tableName, IEnumerable<IDictionary<string, object>> data, IDbTransaction transaction)
        {
            var table = adapter.GetSchema().FindTable(tableName);
            var columns = table.Columns.Where(c => !c.IsIdentity).ToList();

            string columnList = string.Join(",", columns.Select(c => c.QuotedName));
            string valueList = string.Join(",", columns.Select(c => "?"));

            string insertSql = "insert into " + table.QualifiedName + " (" + columnList + ") values (" + valueList + ")";

            var identityFunction = adapter.GetIdentityFunction();
            if (!string.IsNullOrWhiteSpace(identityFunction))
            {
                var identityColumn = table.Columns.FirstOrDefault(col => col.IsIdentity);

                if (identityColumn != null)
                {
                    var selectSql = "select * from " + table.QualifiedName + " where " + identityColumn.QuotedName +
                                     " = " + identityFunction;
                    if (adapter.ProviderSupportsCompoundStatements)
                    {
                        return InsertRowsWithCompoundStatement(adapter, data, transaction, table, columns, selectSql, insertSql);
                    }

                    return InsertRowsWithSeparateStatements(adapter, data, transaction, table, columns, insertSql, selectSql);
                }
            }

            InsertRowsWithoutFetchBack(adapter, data, table, columns, insertSql);

            return null;
        }

        private static void InsertRowsWithoutFetchBack(AdoAdapter adapter, IEnumerable<IDictionary<string, object>> data, Table table, List<Column> columns,
                                                       string insertSql)
        {
            using (var connection = adapter.CreateConnection())
            {
                using (var insertCommand = new CommandHelper(adapter.SchemaProvider).Create(connection, insertSql))
                {
                    connection.Open();
                    foreach (var row in data)
                    {
                        InsertRow(row, columns, table, insertCommand);
                    }
                }
            }
        }

        private static IEnumerable<IDictionary<string, object>> InsertRowsWithSeparateStatements(AdoAdapter adapter, IEnumerable<IDictionary<string, object>> data,
                                                                    IDbTransaction transaction, Table table, List<Column> columns,
                                                                    string insertSql, string selectSql)
        {
            if (transaction != null)
            {
                var insertCommand = new CommandHelper(adapter.SchemaProvider).Create(transaction.Connection, insertSql);
                var selectCommand = transaction.Connection.CreateCommand();
                selectCommand.CommandText = selectSql;
                insertCommand.Transaction = transaction;
                selectCommand.Transaction = transaction;
                return data.Select(row => InsertRow(row, columns, table, insertCommand, selectCommand)).ToList();
            }

            using (var connection = adapter.CreateConnection())
            {
                using (var insertCommand = new CommandHelper(adapter.SchemaProvider).Create(connection, insertSql))
                using (var selectCommand = connection.CreateCommand())
                {
                    selectCommand.CommandText = selectSql;
                    connection.Open();
                    return data.Select(row => InsertRow(row, columns, table, insertCommand, selectCommand)).ToList();
                }
            }
        }

        private static IEnumerable<IDictionary<string, object>> InsertRowsWithCompoundStatement(AdoAdapter adapter, IEnumerable<IDictionary<string, object>> data,
                                                                   IDbTransaction transaction, Table table, List<Column> columns,
                                                                   string selectSql, string insertSql)
        {
            insertSql += "; " + selectSql;
            if (transaction != null)
            {
                var command = new CommandHelper(adapter.SchemaProvider).Create(transaction.Connection, insertSql);
                command.Transaction = transaction;
                return data.Select(row => InsertRowAndSelect(row, columns, table, command)).ToList();
            }

            using (var connection = adapter.CreateConnection())
            {
                using (var command = new CommandHelper(adapter.SchemaProvider).Create(connection, insertSql))
                {
                    connection.Open();
                    return data.Select(row => InsertRowAndSelect(row, columns, table, command)).ToList();
                }
            }
        }

        private static IDictionary<string, object> InsertRowAndSelect(IDictionary<string, object> row, List<Column> columns, Table table, IDbCommand command)
        {
            var values = new object[command.Parameters.Count];
            foreach (var kvp in row)
            {
                int index = columns.IndexOf(table.FindColumn(kvp.Key));
                if (index == -1)
                {
                    values[index] = kvp.Value;
                }
            }

            CommandHelper.SetParameterValues(command, values);
            var insertedRow = TryExecuteSingletonQuery(command);
            return insertedRow;
        }

        private static int InsertRow(IDictionary<string, object> row, List<Column> columns, Table table, IDbCommand command)
        {
            var values = new object[command.Parameters.Count];
            foreach (var kvp in row)
            {
                int index = columns.IndexOf(table.FindColumn(kvp.Key));
                if (index > -1)
                {
                    values[index] = kvp.Value;
                }
            }

            CommandHelper.SetParameterValues(command, values);
            return TryExecute(command);
        }

        private static IDictionary<string, object> InsertRow(IDictionary<string, object> row, List<Column> columns, Table table, IDbCommand insertCommand, IDbCommand selectCommand)
        {
            var values = new object[insertCommand.Parameters.Count];
            foreach (var kvp in row)
            {
                int index = columns.IndexOf(table.FindColumn(kvp.Key));
                if (index == -1)
                {
                    values[index] = kvp.Value;
                }
            }

            CommandHelper.SetParameterValues(insertCommand, values);
            if (TryExecute(insertCommand) == 1)
                return TryExecuteSingletonQuery(selectCommand);
            return null;
        }

        private static IDictionary<string, object> TryExecuteSingletonQuery(IDbCommand command)
        {
            command.WriteTrace();
            try
            {
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return reader.ToDictionary();
                    }
                }
            }
            catch (DbException ex)
            {
                throw new AdoAdapterException(ex.Message, command);
            }

            return null;
        }

        private static int TryExecute(IDbCommand command)
        {
            command.WriteTrace();
            try
            {
                return command.ExecuteNonQuery();
            }
            catch (DbException ex)
            {
                throw new AdoAdapterException(ex.Message, command);
            }
        }
    }
}
