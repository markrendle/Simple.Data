namespace Simple.Data
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using QueryPolyfills;

    public partial class InMemoryAdapter : Adapter, IAdapterWithFunctions
    {
        private readonly Dictionary<string, string> _autoIncrementColumns;
        private readonly Dictionary<string, string[]> _keyColumns;

        private readonly Dictionary<string, List<IDictionary<string, object>>> _tables;

        [Flags]
        private enum FunctionFlags
        {
            None = 0x00000000,
            PassThru = 0x00000001
        }

        private class FunctionInfo
        {
            public FunctionInfo(Delegate func, FunctionFlags flags)
            {
                Delegate = func;
                Flags = flags;
            }

            public Delegate Delegate { get; private set; }
            public FunctionFlags Flags { get; private set; }
        }

        private readonly Dictionary<string, FunctionInfo> _functions = new Dictionary<string, FunctionInfo>(); 

        private readonly ICollection<JoinInfo> _joins = new Collection<JoinInfo>();

        public InMemoryAdapter() : this(StringComparer.OrdinalIgnoreCase)
        {
        }

        public InMemoryAdapter(IEqualityComparer<string> nameComparer)
        {
            _nameComparer = nameComparer;
            _keyColumns = new Dictionary<string, string[]>(_nameComparer);
            _autoIncrementColumns = new Dictionary<string, string>(_nameComparer);
            _tables = new Dictionary<string, List<IDictionary<string, object>>>(nameComparer);
        }

        private List<IDictionary<string, object>> GetTable(string tableName)
        {
            tableName = tableName.ToLowerInvariant();
            if (!_tables.ContainsKey(tableName)) _tables.Add(tableName, new List<IDictionary<string, object>>());
            return _tables[tableName];
        }

        public override IDictionary<string, object> GetKey(string tableName, IDictionary<string, object> record)
        {
            if (!_keyColumns.ContainsKey(tableName)) return null;
            return _keyColumns[tableName].ToDictionary(key => key, key => record.ContainsKey(key) ? record[key] : null);
        }

        public override IList<string> GetKeyNames(string tableName)
        {
            if (!_keyColumns.ContainsKey(tableName)) return null;
            return _keyColumns[tableName];
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
            var whereClauseHandler = new WhereClauseHandler(tableName, new WhereClause(criteria));
            return whereClauseHandler.Run(GetTable(tableName));
        }

        public override IEnumerable<IDictionary<string, object>> RunQuery(SimpleQuery query,
                                                                          out IEnumerable<SimpleQueryClauseBase>
                                                                              unhandledClauses)
        {
            unhandledClauses = query.Clauses.AsEnumerable();
            return GetTable(query.TableName);
        }

        public override IDictionary<string, object> Insert(string tableName, IDictionary<string, object> data, bool resultRequired)
        {
            data = new Dictionary<string, object>(data, _nameComparer);
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
                    nextVal = table.Select(d => d[autoIncrementColumn]).Max();
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

        private static void UpdateRecord(IEnumerable<KeyValuePair<string, object>> data, IDictionary<string, object> record)
        {
            foreach (var kvp in data)
            {
                record[kvp.Key] = kvp.Value;
            }
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
            return (functionName.Equals("like", StringComparison.OrdinalIgnoreCase) ||
                    functionName.Equals("notlike", StringComparison.OrdinalIgnoreCase))
                   && args.Length == 1
                   && args[0] is string;
        }

        public void SetAutoIncrementColumn(string tableName, string columnName)
        {
            _autoIncrementColumns.Add(tableName, columnName);
        }

        public void SetKeyColumn(string tableName, string columnName)
        {
            _keyColumns[tableName] = new[] {columnName};
        }

        public void SetAutoIncrementKeyColumn(string tableName, string columnName)
        {
            SetKeyColumn(tableName, columnName);
            SetAutoIncrementColumn(tableName, columnName);
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
            var join = new JoinInfo(masterTableName, masterKey, masterPropertyName, detailTableName, detailKey,
                                detailPropertyName);
            _joins.Add(join);
        }

        public JoinConfig Join
        {
            get { return new JoinConfig(_joins);}
        }

        private IEqualityComparer<string> _nameComparer = EqualityComparer<string>.Default;

        internal class JoinInfo
        {
            private readonly string _masterTableName;
            private readonly string _masterKey;
            private readonly string _masterPropertyName;
            private readonly string _detailTableName;
            private readonly string _detailKey;
            private readonly string _detailPropertyName;

            public JoinInfo(string masterTableName, string masterKey, string masterPropertyName, string detailTableName, string detailKey, string detailPropertyName)
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

        public override IDictionary<string, object> Upsert(string tableName, IDictionary<string, object> dict, SimpleExpression criteriaExpression, bool isResultRequired, IAdapterTransaction adapterTransaction)
        {
            return Upsert(tableName, dict, criteriaExpression, isResultRequired);
        }

        public override IEnumerable<IDictionary<string, object>> UpsertMany(string tableName, IList<IDictionary<string, object>> list, IEnumerable<string> keyFieldNames, IAdapterTransaction adapterTransaction, bool isResultRequired, Func<IDictionary<string,object>,Exception,bool> errorCallback)
        {
            return UpsertMany(tableName, list, keyFieldNames, isResultRequired, errorCallback);
        }

        public override IEnumerable<IDictionary<string, object>> UpsertMany(string tableName, IList<IDictionary<string, object>> list, IAdapterTransaction adapterTransaction, bool isResultRequired, Func<IDictionary<string, object>, Exception, bool> errorCallback)
        {
            return UpsertMany(tableName, list, isResultRequired, errorCallback);
        }

        public IDictionary<string, object> Get(string tableName, IAdapterTransaction transaction, params object[] parameterValues)
        {
            return Get(tableName, parameterValues);
        }

        public class JoinConfig
        {
            private readonly ICollection<JoinInfo> _joins;
            private JoinInfo _joinInfo;

            internal JoinConfig(ICollection<JoinInfo> joins)
            {
                _joins = joins;
                _joinInfo = new JoinInfo(null,null,null,null,null,null);
            }

            public JoinConfig Master(string tableName, string keyName, string propertyNameInDetailRecords = null)
            {
                if (_joins.Contains(_joinInfo)) _joins.Remove(_joinInfo);
                _joinInfo = new JoinInfo(tableName, keyName, propertyNameInDetailRecords ?? tableName, _joinInfo.DetailTableName,
                                 _joinInfo.DetailKey, _joinInfo.DetailPropertyName);
                _joins.Add(_joinInfo);
                return this;
            }


            public JoinConfig Detail(string tableName, string keyName, string propertyNameInMasterRecords = null)
            {
                if (_joins.Contains(_joinInfo)) _joins.Remove(_joinInfo);
                _joinInfo = new JoinInfo(_joinInfo.MasterTableName, _joinInfo.MasterKey, _joinInfo.MasterPropertyName,
                                         tableName, keyName,
                                         propertyNameInMasterRecords ?? tableName);
                _joins.Add(_joinInfo);
                return this;
            }
        }

        public void AddFunction<TResult>(string functionName, Func<TResult> function)
        {
            _functions.Add(functionName, new FunctionInfo(function, FunctionFlags.None));
        }

        public void AddFunction<T,TResult>(string functionName, Func<T,TResult> function)
        {
            _functions.Add(functionName, new FunctionInfo(function, FunctionFlags.None));
        }

        public void AddFunction<T1,T2,TResult>(string functionName, Func<T1,T2,TResult> function)
        {
            _functions.Add(functionName, new FunctionInfo(function, FunctionFlags.None));
        }

        public void AddFunction<T1,T2,T3,TResult>(string functionName, Func<T1,T2,T3,TResult> function)
        {
            _functions.Add(functionName, new FunctionInfo(function, FunctionFlags.None));
        }

        public void AddFunction<T1, T2, T3, T4, TResult>(string functionName, Func<T1, T2, T3, T4, TResult> function)
        {
            _functions.Add(functionName, new FunctionInfo(function, FunctionFlags.None));
        }

        public void AddDelegate(string functionName, Delegate function)
        {
            _functions.Add(functionName, new FunctionInfo(function, FunctionFlags.None));
        }

        public void AddFunction<TResult>(string functionName, Func<IDictionary<string, object>, TResult> function)
        {
            _functions.Add(functionName, new FunctionInfo(function, FunctionFlags.PassThru));
        }

        public bool IsValidFunction(string functionName)
        {
            return _functions.ContainsKey(functionName);
        }

        public IEnumerable<IEnumerable<IEnumerable<KeyValuePair<string, object>>>> Execute(string functionName, IDictionary<string, object> parameters)
        {
            if (!_functions.ContainsKey(functionName)) throw new InvalidOperationException(string.Format("Function '{0}' not found.", functionName));
            var obj = ((_functions[functionName].Flags & FunctionFlags.PassThru) == FunctionFlags.PassThru) ?
                            _functions[functionName].Delegate.DynamicInvoke(parameters) :
                            _functions[functionName].Delegate.DynamicInvoke(parameters.Values.ToArray());

            var dict = obj as IDictionary<string, object>;
            if (dict != null) return new List<IEnumerable<IDictionary<string, object>>> { new List<IDictionary<string, object>> { dict } };

            var list = obj as IEnumerable<IDictionary<string, object>>;
            if (list != null) return new List<IEnumerable<IDictionary<string, object>>> { list };

            return obj as IEnumerable<IEnumerable<IDictionary<string, object>>>;
        }

        public IEnumerable<IEnumerable<IEnumerable<KeyValuePair<string, object>>>> Execute(string functionName, IDictionary<string, object> parameters, IAdapterTransaction transaction)
        {
            return Execute(functionName, parameters);
        }
    }
}