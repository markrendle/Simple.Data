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

        public virtual void InsertRowsWithoutFetchBack(string insertSql, Func<IDictionary<string, object>, Exception, bool> onError)
        {
            var connection = Adapter.CreateConnection();
            using (connection.MaybeDisposable())
            {
                using (var insertCommand = new CommandHelper(Adapter).CreateInsert(connection, insertSql, _columns))
                {
                    connection.OpenIfClosed();
                    TryPrepare(insertCommand);
                    foreach (var row in Data)
                    {
                        InsertRow(row, insertCommand, onError);
                    }
                }
            }
        }

        public virtual IEnumerable<IDictionary<string, object>> InsertRowsWithSeparateStatements(string insertSql, string selectSql, Func<IDictionary<string, object>, Exception, bool> onError)
        {
            var connection = Adapter.CreateConnection();
            using (connection.MaybeDisposable())
            {
                using (var insertCommand = new CommandHelper(Adapter).CreateInsert(connection, insertSql, _columns))
                using (var selectCommand = connection.CreateCommand())
                {
                    selectCommand.CommandText = selectSql;
                    connection.OpenIfClosed();
                    TryPrepare(insertCommand, selectCommand);
                    return Data.Select(row => InsertRow(row, insertCommand, selectCommand, onError)).Where(r => r != null).ToList();
                }
            }
        }

        public virtual IEnumerable<IDictionary<string, object>> InsertRowsWithCompoundStatement(string insertSql, string selectSql, Func<IDictionary<string, object>, Exception, bool> onError)
        {
            insertSql += "; " + selectSql;

            var connection = Adapter.CreateConnection();
            using (connection.MaybeDisposable())
            {
                using (var command = new CommandHelper(Adapter).CreateInsert(connection, insertSql, _columns))
                {
                    connection.OpenIfClosed();
                    TryPrepare(command);
                    return Data.Select(row => InsertRowAndSelect(row, command, onError)).Where(r => r != null).ToList();
                }
            }
        }

        protected IDictionary<string, object> InsertRowAndSelect(IDictionary<string, object> row, IDbCommand command, Func<IDictionary<string,object>, Exception, bool> onError)
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
            try
            {
                var insertedRow = TryExecuteSingletonQuery(command);
                return insertedRow;
            }
            catch (Exception ex)
            {
                if (onError(row, ex)) return null;
                throw;
            }
        }

        protected int InsertRow(IDictionary<string, object> row, IDbCommand command, Func<IDictionary<string, object>, Exception, bool> onError)
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
            try
            {
                return TryExecute(command);
            }
            catch (Exception ex)
            {
                if (onError(row, ex)) return 0;
                throw;
            }
        }

        protected IDictionary<string, object> InsertRow(IDictionary<string, object> row, IDbCommand insertCommand, IDbCommand selectCommand, Func<IDictionary<string, object>, Exception, bool> onError)
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
            try
            {
                if (TryExecute(insertCommand) == 1)
                    return TryExecuteSingletonQuery(selectCommand);
            }
            catch (Exception ex)
            {
                if (onError(row, ex)) return null;
                throw;
            }
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