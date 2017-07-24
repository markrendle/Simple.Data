using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shitty.Data.SqlServer
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
            var sqlBulkCopyOptions = BuildBulkCopyOptions(adapter);

            if (transaction != null)
            {
                connection = (SqlConnection) transaction.Connection;
                bulkCopy = new SqlBulkCopy(connection, sqlBulkCopyOptions, (SqlTransaction)transaction);
            }
            else
            {
                connection = (SqlConnection) adapter.CreateConnection();
                bulkCopy = new SqlBulkCopy(connection, sqlBulkCopyOptions, null);
            }

            bulkCopy.DestinationTableName = adapter.GetSchema().FindTable(tableName).QualifiedName;

            using (connection.MaybeDisposable())
            using (bulkCopy)
            {
                connection.OpenIfClosed();

                var dataList = data.ToList();
                foreach (var record in dataList)
                {
                    if (count == 0)
                    {
                        dataTable = CreateDataTable(adapter, tableName, dataList.SelectMany(r => r.Keys).Distinct(), bulkCopy);
                    }
                    AddRow(dataTable, record);

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

        private SqlBulkCopyOptions BuildBulkCopyOptions(AdoAdapter adapter)
        {
            var options = SqlBulkCopyOptions.Default;

            if (adapter.AdoOptions != null)
            {
                options |= (adapter.AdoOptions.FireTriggersOnBulkInserts
                                ? SqlBulkCopyOptions.FireTriggers
                                : SqlBulkCopyOptions.Default);
            }

            return options;
        }

        private DataTable CreateDataTable(AdoAdapter adapter, string tableName, IEnumerable<string> keys, SqlBulkCopy bulkCopy)
        {
            var table = adapter.GetSchema().FindTable(tableName);
            var dataTable = new DataTable(table.ActualName);

            foreach (var key in keys)
            {
                if (table.HasColumn(key))
                {
                    var column = (SqlColumn)table.FindColumn(key);
                    dataTable.Columns.Add(column.ActualName, column.SqlDbType.ToClrType());
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

        private void AddRow(DataTable dataTable, IDictionary<string, object> record)
        {
            var dataRow = dataTable.NewRow();
            foreach (DataColumn column in dataTable.Columns)
            {
                if (record.ContainsKey(column.ColumnName))
                    dataRow[column] = record[column.ColumnName];
            }
            dataTable.Rows.Add(dataRow);
        }

    }
}
