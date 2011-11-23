using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.Ado
{
    using System.Data;
    using Schema;

    class BulkUpdater : IBulkUpdater
    {
        public int Update(AdoAdapter adapter, string tableName, IList<IDictionary<string, object>> data, IDbTransaction transaction)
        {
            return Update(adapter, tableName, data, adapter.GetKeyFieldNames(tableName).ToList(), transaction);
        }

        public int Update(AdoAdapter adapter, string tableName, IList<IDictionary<string, object>> data, IEnumerable<string> criteriaFieldNames, IDbTransaction transaction)
        {
            int count = 0;
            if (data == null || !data.Any()) 
                return count;

            var criteriaFieldNameList = criteriaFieldNames.ToList();
            if (criteriaFieldNameList.Count == 0) throw new NotSupportedException("Adapter does not support key-based update for this object.");

            if (!AllRowsHaveSameKeys(data)) throw new SimpleDataException("Records have different structures. Bulk updates are only valid on consistent records.");
            var table = adapter.GetSchema().FindTable(tableName);

            var exampleRow = new Dictionary<string, object>(data.First(), HomogenizedEqualityComparer.DefaultInstance);

            var commandBuilder = new UpdateHelper(adapter.GetSchema()).GetUpdateCommand(tableName, exampleRow,
                                                                    ExpressionHelper.CriteriaDictionaryToExpression(
                                                                        tableName, GetCriteria(criteriaFieldNameList, exampleRow)));

            var connection = adapter.CreateConnection();
            using (connection.MaybeDisposable())
            using (var command = commandBuilder.GetRepeatableCommand(connection))
            {
                var propertyToParameterMap = CreatePropertyToParameterMap(data, table, command);

                foreach (var row in data)
                {
                    foreach (var kvp in row)
                    {
                        propertyToParameterMap[kvp.Key].Value = kvp.Value ?? DBNull.Value;
                    }
                    count += command.ExecuteNonQuery();
                }
            }

            return count;
        }

        private static Dictionary<string, IDbDataParameter> CreatePropertyToParameterMap(IEnumerable<IDictionary<string, object>> data, Table table, IDbCommand command)
        {
            return data.First().Select(kvp => new
            {
                kvp.Key,
                Value = GetDbDataParameter(table, command, kvp)
            })
                .Where(t => t.Value != null)
                .ToDictionary(t => t.Key, t => t.Value);
        }

        private static IDbDataParameter GetDbDataParameter(Table table, IDbCommand command, KeyValuePair<string, object> kvp)
        {
            return command.Parameters.Cast<IDbDataParameter>().
                FirstOrDefault
                (p =>
                 p.SourceColumn ==
                 table.FindColumn(kvp.Key).ActualName);
        }

        private static bool AllRowsHaveSameKeys(IList<IDictionary<string, object>> data)
        {
            var exemplar = new HashSet<string>(data.First().Keys);

            return data.Skip(1).All(d => exemplar.SetEquals(d.Keys));
        }

        private static Dictionary<string, object> GetCriteria(IEnumerable<string> keyFieldNames, IDictionary<string, object> record)
        {
            var criteria = new Dictionary<string, object>();

            foreach (var keyFieldName in keyFieldNames)
            {
                if (!record.ContainsKey(keyFieldName))
                {
                    throw new InvalidOperationException("Key field value not set.");
                }

                criteria.Add(keyFieldName, record[keyFieldName]);
                record.Remove(keyFieldName);
            }
            return criteria;
        }
    }
}
