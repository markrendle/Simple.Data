namespace Simple.Data.Ado
{
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using Schema;

    class BulkInserterTransactionHelper : BulkInserterHelper
    {
        private readonly IDbTransaction _transaction;

        public BulkInserterTransactionHelper(IDbTransaction transaction)
        {
            _transaction = transaction;
        }

        public override IEnumerable<IDictionary<string, object>> InsertRowsWithSeparateStatements(AdoAdapter adapter, IEnumerable<IDictionary<string, object>> data, Table table, List<Column> columns, string insertSql, string selectSql)
        {
            var insertCommand = new CommandHelper(adapter.SchemaProvider).Create(_transaction.Connection, insertSql);
            var selectCommand = _transaction.Connection.CreateCommand();
            selectCommand.CommandText = selectSql;
            insertCommand.Transaction = _transaction;
            selectCommand.Transaction = _transaction;
            return data.Select(row => InsertRow(row, columns, table, insertCommand, selectCommand)).ToList();
        }

        public override IEnumerable<IDictionary<string, object>> InsertRowsWithCompoundStatement(AdoAdapter adapter, IEnumerable<IDictionary<string, object>> data, Table table, List<Column> columns, string selectSql, string insertSql)
        {
            insertSql += "; " + selectSql;
            var command = new CommandHelper(adapter.SchemaProvider).Create(_transaction.Connection, insertSql);
            command.Transaction = _transaction;
            return data.Select(row => InsertRowAndSelect(row, columns, table, command)).ToList();
        }

        public override void InsertRowsWithoutFetchBack(AdoAdapter adapter, IEnumerable<IDictionary<string, object>> data, Table table, List<Column> columns, string insertSql)
        {
            using (var insertCommand = new CommandHelper(adapter.SchemaProvider).Create(_transaction.Connection, insertSql))
            {
                insertCommand.Transaction = _transaction;
                foreach (var row in data)
                {
                    InsertRow(row, columns, table, insertCommand);
                }
            }

        }
    }
}