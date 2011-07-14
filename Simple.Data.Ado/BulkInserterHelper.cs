namespace Simple.Data.Ado
{
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Linq;
    using Schema;

    class BulkInserterHelper
    {
        public virtual void InsertRowsWithoutFetchBack(AdoAdapter adapter, IEnumerable<IDictionary<string, object>> data, Table table, List<Column> columns,
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

        public virtual IEnumerable<IDictionary<string, object>> InsertRowsWithSeparateStatements(AdoAdapter adapter, IEnumerable<IDictionary<string, object>> data,
                                                                                                 Table table, List<Column> columns,
                                                                                                 string insertSql, string selectSql)
        {
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

        public virtual IEnumerable<IDictionary<string, object>> InsertRowsWithCompoundStatement(AdoAdapter adapter, IEnumerable<IDictionary<string, object>> data,
                                                                                                Table table, List<Column> columns,
                                                                                                string selectSql, string insertSql)
        {
            insertSql += "; " + selectSql;

            using (var connection = adapter.CreateConnection())
            {
                using (var command = new CommandHelper(adapter.SchemaProvider).Create(connection, insertSql))
                {
                    connection.Open();
                    return data.Select(row => InsertRowAndSelect(row, columns, table, command)).ToList();
                }
            }
        }

        protected static IDictionary<string, object> InsertRowAndSelect(IDictionary<string, object> row, List<Column> columns, Table table, IDbCommand command)
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
            var insertedRow = TryExecuteSingletonQuery(command);
            return insertedRow;
        }

        protected static int InsertRow(IDictionary<string, object> row, List<Column> columns, Table table, IDbCommand command)
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

        protected static IDictionary<string, object> InsertRow(IDictionary<string, object> row, List<Column> columns, Table table, IDbCommand insertCommand, IDbCommand selectCommand)
        {
            var values = new object[insertCommand.Parameters.Count];
            foreach (var kvp in row)
            {
                int index = columns.IndexOf(table.FindColumn(kvp.Key));
                if (index > -1)
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