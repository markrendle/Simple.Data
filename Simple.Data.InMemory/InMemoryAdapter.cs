using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.InMemory
{
    using QueryPolyfills;

    public class InMemoryAdapter : Adapter
    {
        private readonly Dictionary<string, List<IDictionary<string,object>>> _tables = new Dictionary<string, List<IDictionary<string, object>>>();
        private readonly Dictionary<string,string> _autoIncrementColumns = new Dictionary<string, string>();
        private readonly Dictionary<string, string[]> _keyColumns = new Dictionary<string, string[]>(); 

        private List<IDictionary<string,object>> GetTable(string tableName)
        {
            tableName = tableName.ToLowerInvariant();
            if (!_tables.ContainsKey(tableName)) _tables.Add(tableName, new List<IDictionary<string, object>>());
            return _tables[tableName];
        }

        public override IEnumerable<IDictionary<string, object>> Find(string tableName, SimpleExpression criteria)
        {
            var whereClauseHandler = new WhereClauseHandler(new WhereClause(criteria));
            return whereClauseHandler.Run(GetTable(tableName));
        }

        public override IEnumerable<IDictionary<string, object>> RunQuery(SimpleQuery query, out IEnumerable<SimpleQueryClauseBase> unhandledClauses)
        {
            unhandledClauses = query.Clauses.AsEnumerable();
            return GetTable(query.TableName);
        }

        public override IDictionary<string, object> Insert(string tableName, IDictionary<string, object> data)
        {
            if (_autoIncrementColumns.ContainsKey(tableName))
            {
                object nextVal = GetTable(tableName).Select(d => d[_autoIncrementColumns[tableName]]).Max();
                nextVal = ObjectMaths.Increment(nextVal);
                data[_autoIncrementColumns[tableName]] = nextVal;
            }
            GetTable(tableName).Add(data);
            return data;
        }

        public override int Update(string tableName, IDictionary<string, object> data, SimpleExpression criteria)
        {
            int count = 0;
            foreach (var record in Find(tableName, criteria))
            {
                UpdateRecord(data, record);
                ++count;
            }
            return count;
        }

        private static void UpdateRecord(IDictionary<string, object> data, IDictionary<string, object> record)
        {
            foreach (var kvp in data)
            {
                record[kvp.Key] = kvp.Value;
            }
        }

        public override int Update(string tableName, IDictionary<string, object> data)
        {
            if (!_keyColumns.ContainsKey(tableName)) throw new InvalidOperationException("No key column(s) specified.");
            IDictionary<string, object> row = null;
            if (_keyColumns[tableName].Length == 1)
            {
                row =
                    GetTable(tableName).Single(
                        d => d[_keyColumns[tableName][0]] == data[_keyColumns[tableName][0]]);
            }
            else
            {
                IEnumerable<IDictionary<string, object>> rows = GetTable(tableName);
                rows = _keyColumns[tableName].Aggregate(rows, (current, keyColumn) => current.Where(d => d[keyColumn] == data[keyColumn]));
                row = rows.Single();
            }
            UpdateRecord(data, row);
            return 1;
        }

        public override int Delete(string tableName, SimpleExpression criteria)
        {
            int count = 0;
            foreach (var record in Find(tableName, criteria))
            {
                GetTable(tableName).Remove(record);
                ++count;
            }
            return count;
        }

        public override bool IsExpressionFunction(string functionName, params object[] args)
        {
            return functionName.Equals("like", StringComparison.OrdinalIgnoreCase) && args.Length == 1 && args[0] is string;
        }

        public void SetAutoIncrementColumn(string tableName, string columnName)
        {
            _autoIncrementColumns.Add(tableName, columnName);
        }

        public void SetKeyColumn(string tableName, string columnName)
        {
            _keyColumns[tableName] = new[] { columnName };
        }

        public void SetKeyColumns(string tableName, params string[] columnNames)
        {
            _keyColumns[tableName] = columnNames;
        }
    }
}
