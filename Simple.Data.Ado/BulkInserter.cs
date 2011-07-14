namespace Simple.Data.Ado
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Data;

    class BulkInserter : IBulkInserter
    {
        public IEnumerable<IDictionary<string, object>> Insert(AdoAdapter adapter, string tableName, IEnumerable<IDictionary<string, object>> data, IDbTransaction transaction)
        {
            var helper = transaction == null ? new BulkInserterHelper() : new BulkInserterTransactionHelper(transaction);

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
                        return helper.InsertRowsWithCompoundStatement(adapter, data, table, columns, selectSql, insertSql);
                    }

                    return helper.InsertRowsWithSeparateStatements(adapter, data, table, columns, insertSql, selectSql);
                }
            }

            helper.InsertRowsWithoutFetchBack(adapter, data, table, columns, insertSql);

            return null;
        }
    }
}
