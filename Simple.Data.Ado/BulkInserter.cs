namespace Simple.Data.Ado
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Data;
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

            var helper = transaction == null
                             ? new BulkInserterHelper(adapter, data, table, columns)
                             : new BulkInserterTransactionHelper(adapter, data, table, columns, transaction);

            var identityColumn = table.Columns.FirstOrDefault(col => col.IsIdentity);
            if (identityColumn != null)
            {
                var identityFunction = adapter.GetIdentityFunction();
                if (!string.IsNullOrWhiteSpace(identityFunction))
                {
                    return InsertRowsAndReturn(adapter, identityFunction, helper, insertSql, table);
                }
            }

            helper.InsertRowsWithoutFetchBack(insertSql);

            return null;
        }

        private static IEnumerable<IDictionary<string, object>> InsertRowsAndReturn(AdoAdapter adapter, string identityFunction, BulkInserterHelper helper,
                                                string insertSql, Table table)
        {
            var identityColumn = table.Columns.FirstOrDefault(col => col.IsIdentity);

            if (identityColumn != null)
            {
                var selectSql = "select * from " + table.QualifiedName + " where " + identityColumn.QuotedName +
                                " = " + identityFunction;
                if (adapter.ProviderSupportsCompoundStatements)
                {
                    return helper.InsertRowsWithCompoundStatement(insertSql, selectSql);
                }
                return helper.InsertRowsWithSeparateStatements(insertSql, selectSql);
            }

            return null;
        }
    }
}
