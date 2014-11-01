namespace Simple.Data.Ado
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;
    using Schema;

    class BulkInserterTransactionHelper : BulkInserterHelper
    {
        private readonly IDbTransaction _transaction;

        public BulkInserterTransactionHelper(AdoAdapter adapter, IEnumerable<IReadOnlyDictionary<string, object>> data, Table table, List<Column> columns, IDbTransaction transaction)
            : base(adapter, data, table, columns)
        {
            _transaction = transaction;
        }

        public override async Task<IEnumerable<IDictionary<string, object>>> InsertRowsWithSeparateStatements(string insertSql, string selectSql, ErrorCallback onError)
        {
            var insertCommand = new CommandHelper(Adapter).Create(_transaction.Connection, insertSql);
            var selectCommand = _transaction.Connection.CreateCommand(Adapter.AdoOptions);
            selectCommand.CommandText = selectSql;
            insertCommand.Transaction = _transaction;
            selectCommand.Transaction = _transaction;
            var list = new List<IDictionary<string, object>>();
            foreach (var row in Data)
            {
                list.Add(await InsertRow(row, insertCommand, selectCommand, onError));
            }
            return list.Where(r => r != null);
        }

        public override async Task<IEnumerable<IDictionary<string, object>>> InsertRowsWithCompoundStatement(string insertSql, string selectSql, ErrorCallback onError)
        {
            insertSql += "; " + selectSql;
            var command = new CommandHelper(Adapter).Create(_transaction.Connection, insertSql);
            command.Transaction = _transaction;
            var list = new List<IDictionary<string, object>>();
            foreach (var row in Data)
            {
                list.Add(await InsertRowAndSelect(row, command, onError));
            }
            return list.Where(r => r != null);
        }

        public override async Task InsertRowsWithoutFetchBack(string insertSql, ErrorCallback onError)
        {
            using (var insertCommand = new CommandHelper(Adapter).Create(_transaction.Connection, insertSql))
            {
                insertCommand.Transaction = _transaction;
                foreach (var row in Data)
                {
                    await InsertRow(row, insertCommand, onError);
                }
            }

        }
    }
}