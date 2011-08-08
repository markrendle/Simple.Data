namespace Simple.Data.Ado
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Diagnostics;
    using System.Linq;
    using Schema;

    class BulkInserterHelper
    {
        protected readonly AdoAdapter Adapter;
        protected readonly IEnumerable<IDictionary<string, object>> Data;
        private readonly Table _table;
        private readonly List<Column> _columns;

        public BulkInserterHelper(AdoAdapter adapter, IEnumerable<IDictionary<string, object>> data, Table table, List<Column> columns)
        {
            Adapter = adapter;
            Data = data;
            _table = table;
            _columns = columns;
        }

        public virtual void InsertRowsWithoutFetchBack(string insertSql)
        {
            using (var connection = Adapter.CreateConnection())
            {
                using (var insertCommand = new CommandHelper(Adapter.SchemaProvider).CreateInsert(connection, insertSql, _columns))
                {
                    connection.Open();
                    TryPrepare(insertCommand);
                    insertCommand.Prepare();
                    foreach (var row in Data)
                    {
                        InsertRow(row, insertCommand);
                    }
                }
            }
        }

        public virtual IEnumerable<IDictionary<string, object>> InsertRowsWithSeparateStatements(string insertSql, string selectSql)
        {
            using (var connection = Adapter.CreateConnection())
            {
                using (var insertCommand = new CommandHelper(Adapter.SchemaProvider).CreateInsert(connection, insertSql, _columns))
                using (var selectCommand = connection.CreateCommand())
                {
                    selectCommand.CommandText = selectSql;
                    connection.Open();
                    TryPrepare(insertCommand, selectCommand);
                    return Data.Select(row => InsertRow(row, insertCommand, selectCommand)).ToList();
                }
            }
        }

        public virtual IEnumerable<IDictionary<string, object>> InsertRowsWithCompoundStatement(string insertSql, string selectSql)
        {
            insertSql += "; " + selectSql;

            using (var connection = Adapter.CreateConnection())
            {
                using (var command = new CommandHelper(Adapter.SchemaProvider).CreateInsert(connection, insertSql, _columns))
                {
                    connection.Open();
                    TryPrepare(command);
                    return Data.Select(row => InsertRowAndSelect(row, command)).ToList();
                }
            }
        }

        protected IDictionary<string, object> InsertRowAndSelect(IDictionary<string, object> row, IDbCommand command)
        {
            var values = new object[command.Parameters.Count];
            foreach (var kvp in row)
            {
                int index = _columns.IndexOf(_table.FindColumn(kvp.Key));
                if (index > -1)
                {
                    values[index] = kvp.Value;
                }
            }

            CommandHelper.SetParameterValues(command, values);
            var insertedRow = TryExecuteSingletonQuery(command);
            return insertedRow;
        }

        protected int InsertRow(IDictionary<string, object> row, IDbCommand command)
        {
            var values = new object[command.Parameters.Count];
            foreach (var kvp in row)
            {
                int index = _columns.IndexOf(_table.FindColumn(kvp.Key));
                if (index > -1)
                {
                    values[index] = kvp.Value;
                }
            }

            CommandHelper.SetParameterValues(command, values);
            return TryExecute(command);
        }

        protected IDictionary<string, object> InsertRow(IDictionary<string, object> row, IDbCommand insertCommand, IDbCommand selectCommand)
        {
            var values = new object[insertCommand.Parameters.Count];
            foreach (var kvp in row)
            {
                int index = _columns.IndexOf(_table.FindColumn(kvp.Key));
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

        private static void TryPrepare(params IDbCommand[] commands)
        {
            foreach (var command in commands)
            {
                try
                {
                    command.Prepare();
                }
                catch (InvalidOperationException)
                {
                    Trace.TraceWarning("Could not prepare command.");
                }
            }
        }
    }
}