namespace Simple.Data.Ado
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Data;
    using System.Threading.Tasks;
    using Schema;

    public class BulkInserter : IBulkInserter
    {
        public async Task<IEnumerable<IDictionary<string, object>>> Insert(AdoAdapter adapter, string tableName, IEnumerable<IReadOnlyDictionary<string, object>> data, IDbTransaction transaction, ErrorCallback onError, bool resultRequired)
        {
            var table = adapter.GetSchema().FindTable(tableName);
            var columns = table.Columns.Where(c => c.IsWriteable).ToList();

            string columnList = string.Join(",", columns.Select(c => c.QuotedName));
            string valueList = string.Join(",", columns.Select(c => "?"));

            string insertSql = "insert into " + table.QualifiedName + " (" + columnList + ") values (" + valueList + ")";

            var helper = transaction == null
                             ? new BulkInserterHelper(adapter, data, table, columns)
                             : new BulkInserterTransactionHelper(adapter, data, table, columns, transaction);

            if (resultRequired)
            {
                var identityColumn = table.Columns.FirstOrDefault(col => col.IsIdentity);
                if (identityColumn != null)
                {
                    var identityFunction = adapter.GetIdentityFunction();
                    if (!string.IsNullOrWhiteSpace(identityFunction))
                    {
                        return await InsertRowsAndReturn(adapter, identityFunction, helper, insertSql, table, onError);
                    }
                }
            }

            helper.InsertRowsWithoutFetchBack(insertSql, onError);

            return null;
        }

        private static Task<IEnumerable<IDictionary<string, object>>> InsertRowsAndReturn(AdoAdapter adapter, string identityFunction, BulkInserterHelper helper, string insertSql, Table table, ErrorCallback onError)
        {
            var identityColumn = table.Columns.FirstOrDefault(col => col.IsIdentity);

            if (identityColumn != null)
            {
                var selectSql = "select * from " + table.QualifiedName + " where " + identityColumn.QuotedName +
                                " = " + identityFunction;
                if (adapter.ProviderSupportsCompoundStatements)
                {
                    return helper.InsertRowsWithCompoundStatement(insertSql, selectSql, onError);
                }
                return helper.InsertRowsWithSeparateStatements(insertSql, selectSql, onError);
            }

            return null;
        }
    }
}
