namespace Simple.Data.Ado
{
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using Schema;

    class BulkInserterTransactionHelper : BulkInserterHelper
    {
        private readonly IDbTransaction _transaction;

        public BulkInserterTransactionHelper(AdoAdapter adapter, IEnumerable<IDictionary<string, object>> data, Table table, List<Column> columns, IDbTransaction transaction)
            : base(adapter, data, table, columns)
        {
            _transaction = transaction;
        }

        public override IEnumerable<IDictionary<string, object>> InsertRowsWithSeparateStatements(string insertSql, string selectSql)
        {
            var insertCommand = new CommandHelper(Adapter).Create(_transaction.Connection, insertSql);
            var selectCommand = _transaction.Connection.CreateCommand();
            selectCommand.CommandText = selectSql;
            insertCommand.Transaction = _transaction;
            selectCommand.Transaction = _transaction;
            return Data.Select(row => InsertRow(row, insertCommand, selectCommand)).ToList();
        }

        public override IEnumerable<IDictionary<string, object>> InsertRowsWithCompoundStatement(string insertSql, string selectSql)
        {
            insertSql += "; " + selectSql;
            var command = new CommandHelper(Adapter).Create(_transaction.Connection, insertSql);
            command.Transaction = _transaction;
            return Data.Select(row => InsertRowAndSelect(row, command)).ToList();
        }

        public override void InsertRowsWithoutFetchBack(string insertSql)
        {
            using (var insertCommand = new CommandHelper(Adapter).Create(_transaction.Connection, insertSql))
            {
                insertCommand.Transaction = _transaction;
                foreach (var row in Data)
                {
                    InsertRow(row, insertCommand);
                }
            }

        }
    }
}