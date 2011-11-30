namespace Simple.Data.Ado
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Data;
    using Schema;

    public class BulkInserter : IBulkInserter
    {
        public IEnumerable<IDictionary<string, object>> Insert(AdoAdapter adapter, string tableName, IEnumerable<IDictionary<string, object>> data, IDbTransaction transaction, Func<IDictionary<string,object>, Exception, bool> onError, bool resultRequired)
        {
            var table = adapter.GetSchema().FindTable(tableName);
            var columns = table.Columns.Where(c => !c.IsIdentity).ToList();

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
                        return InsertRowsAndReturn(adapter, identityFunction, helper, insertSql, table, onError);
                    }
                }
            }

            helper.InsertRowsWithoutFetchBack(insertSql, onError);

            return null;
        }

        private static IEnumerable<IDictionary<string, object>> InsertRowsAndReturn(AdoAdapter adapter, string identityFunction, BulkInserterHelper helper, string insertSql, Table table, Func<IDictionary<string, object>, Exception, bool> onError)
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
