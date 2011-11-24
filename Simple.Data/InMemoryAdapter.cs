namespace Simple.Data
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using QueryPolyfills;

    public class InMemoryAdapter : Adapter
    {
        private readonly Dictionary<string, string> _autoIncrementColumns = new Dictionary<string, string>();
        private readonly Dictionary<string, string[]> _keyColumns = new Dictionary<string, string[]>();

        private readonly Dictionary<string, List<IDictionary<string, object>>> _tables =
            new Dictionary<string, List<IDictionary<string, object>>>();

        private readonly ICollection<Join> _joins = new Collection<Join>();

        private List<IDictionary<string, object>> GetTable(string tableName)
        {
            tableName = tableName.ToLowerInvariant();
            if (!_tables.ContainsKey(tableName)) _tables.Add(tableName, new List<IDictionary<string, object>>());
            return _tables[tableName];
        }

        public override IDictionary<string, object> Get(string tableName, params object[] keyValues)
        {
            if (!_keyColumns.ContainsKey(tableName)) throw new InvalidOperationException("No key specified for In-Memory table.");
            var keys = _keyColumns[tableName];
            if (keys.Length != keyValues.Length) throw new ArgumentException("Incorrect number of values for key.");
            var expression = new ObjectReference(keys[0]) == keyValues[0];
            for (int i = 1; i < keyValues.Length; i++)
            {
                expression = expression && new ObjectReference(keys[i]) == keyValues[i];
            }

            return Find(tableName, expression).FirstOrDefault();
        }

        public override IEnumerable<IDictionary<string, object>> Find(string tableName, SimpleExpression criteria)
        {
            var whereClauseHandler = new WhereClauseHandler(new WhereClause(criteria));
            return whereClauseHandler.Run(GetTable(tableName));
        }

        public override IEnumerable<IDictionary<string, object>> RunQuery(SimpleQuery query,
                                                                          out IEnumerable<SimpleQueryClauseBase>
                                                                              unhandledClauses)
        {
            unhandledClauses = query.Clauses.AsEnumerable();
            return GetTable(query.TableName);
        }

        public override IDictionary<string, object> Insert(string tableName, IDictionary<string, object> data)
        {
            if (_autoIncrementColumns.ContainsKey(tableName))
            {
                var table = GetTable(tableName);
                var autoIncrementColumn = _autoIncrementColumns[tableName];

                if(!data.ContainsKey(autoIncrementColumn))
                {
                    data.Add(autoIncrementColumn, 0);
                }

                object nextVal = 0;
                if(table.Count > 0)
                {
                    nextVal = table.Select(d => d[autoIncrementColumn]).Max(); ;
                }
                
                nextVal = ObjectMaths.Increment(nextVal);
                data[autoIncrementColumn] = nextVal;
            }

            GetTable(tableName).Add(data);

            AddAsDetail(tableName, data);
            AddAsMaster(tableName, data);

            return data;
        }

        private void AddAsDetail(string tableName, IDictionary<string, object> data)
        {
            foreach (var @join in _joins.Where(j => j.DetailTableName.Equals(tableName, StringComparison.OrdinalIgnoreCase)))
            {
                if (!data.ContainsKey(@join.DetailKey)) continue;
                foreach (
                    var master in
                        GetTable(@join.MasterTableName).Where(
                            d => d.ContainsKey(@join.MasterKey) && d[@join.MasterKey].Equals(data[@join.DetailKey])))
                {
                    data[@join.MasterPropertyName] = master;
                    if (!master.ContainsKey(@join.DetailPropertyName))
                    {
                        master.Add(@join.DetailPropertyName, new List<IDictionary<string, object>> {data});
                    }
                    else
                    {
                        ((List<IDictionary<string, object>>) master[@join.DetailPropertyName]).Add(data);
                    }
                }
            }
        }

        private void AddAsMaster(string tableName, IDictionary<string, object> data)
        {
            foreach (var @join in _joins.Where(j => j.MasterTableName.Equals(tableName, StringComparison.OrdinalIgnoreCase)))
            {
                if (!data.ContainsKey(@join.MasterKey)) continue;
                foreach (
                    var detail in
                        GetTable(@join.DetailTableName).Where(
                            d => d.ContainsKey(@join.DetailKey) && d[@join.DetailKey].Equals(data[@join.MasterKey])))
                {
                    detail[@join.MasterPropertyName] = data;
                    if (!data.ContainsKey(@join.DetailPropertyName))
                    {
                        data.Add(@join.DetailPropertyName, new List<IDictionary<string, object>> {data});
                    }
                    else
                    {
                        ((List<IDictionary<string, object>>) data[@join.DetailPropertyName]).Add(data);
                    }
                }
            }
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
                row = _keyColumns[tableName]
                    .Aggregate(rows, (current, keyColumn) => current.Where(d => d[keyColumn] == data[keyColumn]))
                    .Single();
            }
            UpdateRecord(data, row);
            return 1;
        }

        public override int Delete(string tableName, SimpleExpression criteria)
        {
            List<IDictionary<string, object>> deletions = Find(tableName, criteria).ToList();
            foreach (var record in deletions)
            {
                GetTable(tableName).Remove(record);
            }
            return deletions.Count;
        }

        public override bool IsExpressionFunction(string functionName, params object[] args)
        {
            return functionName.Equals("like", StringComparison.OrdinalIgnoreCase) && args.Length == 1 &&
                   args[0] is string;
        }

        public void SetAutoIncrementColumn(string tableName, string columnName)
        {
            _autoIncrementColumns.Add(tableName, columnName);
        }

        public void SetKeyColumn(string tableName, string columnName)
        {
            _keyColumns[tableName] = new[] {columnName};
        }

        public void SetKeyColumns(string tableName, params string[] columnNames)
        {
            _keyColumns[tableName] = columnNames;
        }

        /// <summary>
        /// Set up an implicit join between two tables.
        /// </summary>
        /// <param name="masterTableName">The name of the 'master' table</param>
        /// <param name="masterKey">The 'primary key'</param>
        /// <param name="masterPropertyName">The name to give the lookup property in the detail objects</param>
        /// <param name="detailTableName">The name of the 'master' table</param>
        /// <param name="detailKey">The 'foreign key'</param>
        /// <param name="detailPropertyName">The name to give the collection property in the master object</param>
        public void ConfigureJoin(string masterTableName, string masterKey, string masterPropertyName, string detailTableName, string detailKey, string detailPropertyName)
        {
            var join = new Join(masterTableName, masterKey, masterPropertyName, detailTableName, detailKey,
                                detailPropertyName);
            _joins.Add(join);
        }

        public class Join
        {
            private readonly string _masterTableName;
            private readonly string _masterKey;
            private readonly string _masterPropertyName;
            private readonly string _detailTableName;
            private readonly string _detailKey;
            private readonly string _detailPropertyName;

            public Join(string masterTableName, string masterKey, string masterPropertyName, string detailTableName, string detailKey, string detailPropertyName)
            {
                _masterTableName = masterTableName;
                _masterKey = masterKey;
                _masterPropertyName = masterPropertyName;
                _detailTableName = detailTableName;
                _detailKey = detailKey;
                _detailPropertyName = detailPropertyName;
            }

            public string DetailPropertyName
            {
                get { return _detailPropertyName; }
            }

            public string DetailKey
            {
                get { return _detailKey; }
            }

            public string DetailTableName
            {
                get { return _detailTableName; }
            }

            public string MasterPropertyName
            {
                get { return _masterPropertyName; }
            }

            public string MasterKey
            {
                get { return _masterKey; }
            }

            public string MasterTableName
            {
                get { return _masterTableName; }
            }
        }
    }
}