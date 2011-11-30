using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.SqlServer
{
    using System.ComponentModel.Composition;
    using System.Data;
    using System.Data.SqlClient;
    using Ado;

    [Export(typeof(IBulkInserter))]
    public class SqlBulkInserter : IBulkInserter
    {
        public IEnumerable<IDictionary<string, object>> Insert(AdoAdapter adapter, string tableName, IEnumerable<IDictionary<string, object>> data, IDbTransaction transaction, Func<IDictionary<string, object>, Exception, bool> onError, bool resultRequired)
        {
            if (resultRequired)
            {
                return new BulkInserter().Insert(adapter, tableName, data, transaction, onError, resultRequired);
            }

            int count = 0;
            DataTable dataTable = null;

            SqlConnection connection;
            SqlBulkCopy bulkCopy;

            if (transaction != null)
            {
                connection = (SqlConnection) transaction.Connection;
                bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, (SqlTransaction)transaction);
            }
            else
            {
                connection = (SqlConnection) adapter.CreateConnection();
                bulkCopy = new SqlBulkCopy(connection);
            }

            bulkCopy.DestinationTableName = adapter.GetSchema().FindTable(tableName).ActualName;

            using (connection.MaybeDisposable())
            using (bulkCopy)
            {
                connection.OpenIfClosed();
                foreach (var record in data)
                {
                    if (count == 0)
                    {
                        dataTable = CreateDataTable(adapter, tableName, record.Keys, bulkCopy);
                    }
                    dataTable.Rows.Add(record.Values.ToArray());

                    if (++count%5000 == 0)
                    {
                        bulkCopy.WriteToServer(dataTable);
                        dataTable.Clear();
                    }
                }

                if (dataTable.Rows.Count > 0)
                {
                    bulkCopy.WriteToServer(dataTable);
                }
            }

            return null;
        }

        private DataTable CreateDataTable(AdoAdapter adapter, string tableName, ICollection<string> keys, SqlBulkCopy bulkCopy)
        {
            var table = adapter.GetSchema().FindTable(tableName);
            var dataTable = new DataTable(table.ActualName);

            foreach (var key in keys)
            {
                if (table.HasColumn(key))
                {
                    var column = table.FindColumn(key);
                    dataTable.Columns.Add(column.ActualName, column.DbType.ToClrType());
                    bulkCopy.ColumnMappings.Add(column.ActualName, column.ActualName);
                }
                else
                {
                    // For non-matching columns, add a dummy DataColumn to make inserting rows easier.
                    dataTable.Columns.Add(Guid.NewGuid().ToString("N"));
                }
            }

            return dataTable;
        }
    }
}
