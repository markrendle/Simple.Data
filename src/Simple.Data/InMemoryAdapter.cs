using Simple.Data.Operations;

namespace Simple.Data
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data;
    using System.Linq;
    using System.Runtime.Remoting.Messaging;
    using Extensions;
    using QueryPolyfills;

    public partial class InMemoryAdapter : Adapter
    {
        private static readonly Dictionary<Type, Func<InMemoryAdapter, IOperation, OperationResult>> OperationFunctions
            = new Dictionary<Type, Func<InMemoryAdapter, IOperation, OperationResult>>
                {
                    {typeof(GetOperation), (a,o) => a.Get((GetOperation)o)}
                };
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

        public InMemoryAdapter(IEqualityComparer<string> keyComparer)
        {
            _keyComparer = keyComparer;
            _keyColumns = new Dictionary<string, string[]>(_keyComparer);
            _autoIncrementColumns = new Dictionary<string, string>(_keyComparer);
            _tables = new Dictionary<string, List<IDictionary<string, object>>>(keyComparer);
        }

        public override IEqualityComparer<string> KeyComparer
        {
            get { return _keyComparer; }
        }

        private List<IDictionary<string, object>> GetTable(string tableName)
        {
            tableName = tableName.ToLowerInvariant();
            if (!_tables.ContainsKey(tableName)) _tables.Add(tableName, new List<IDictionary<string, object>>());
            return _tables[tableName];
        }

        public override IReadOnlyDictionary<string, object> GetKey(string tableName, IReadOnlyDictionary<string, object> record)
        {
            if (!_keyColumns.ContainsKey(tableName)) return null;
            return _keyColumns[tableName].ToDictionary(key => key, key => record.ContainsKey(key) ? record[key] : null);
        }

        public override IList<string> GetKeyNames(string tableName)
        {
            if (!_keyColumns.ContainsKey(tableName)) return null;
            return _keyColumns[tableName];
        }

        IAdapterTransaction IAdapterWithTransactions.BeginTransaction(IsolationLevel isolationLevel)
        {
            return BeginTransaction(isolationLevel);
        }

        private IEnumerable<IDictionary<string, object>> Find(string tableName, SimpleExpression criteria)
        {
            var whereClauseHandler = new WhereClauseHandler(tableName, new WhereClause(criteria));
            return whereClauseHandler.Run(GetTable(tableName));
        }

        public override OperationResult Execute(IOperation operation)
        {
            if (operation == null) throw new ArgumentNullException("operation");

            Func<InMemoryAdapter, IOperation, OperationResult> func;
            if (OperationFunctions.TryGetValue(operation.GetType(), out func))
            {
                return func(this, operation);
            }
            throw new NotSupportedException();
        }

        public OperationResult Execute(IOperation operation, IAdapterTransaction transaction)
        {
            return Execute(operation);
        }

        private DataResult Get(GetOperation operation)
        {
            if (!_keyColumns.ContainsKey(operation.TableName)) throw new InvalidOperationException("No key specified for In-Memory table.");
            var keys = _keyColumns[operation.TableName];
            if (keys.Length != operation.KeyValues.Length) throw new ArgumentException("Incorrect number of values for key.");
            var expression = new ObjectReference(keys[0]) == operation.KeyValues[0];
            for (int i = 1; i < operation.KeyValues.Length; i++)
            {
                expression = expression && new ObjectReference(keys[i]) == operation.KeyValues[i];
            }

            return new DataResult(Find(operation.TableName, expression));
        }

        private DataResult RunQuery(SimpleQuery query, out IEnumerable<SimpleQueryClauseBase> unhandledClauses)
        {
            unhandledClauses = query.Clauses.AsEnumerable();
            return new DataResult(GetTable(query.TableName));
        }

        private DataResult Insert(InsertOperation operation)
        {
            return new DataResult(InsertImpl(operation));
        }

        private IEnumerable<IDictionary<string, object>> InsertImpl(InsertOperation operation)
        {
            foreach (var data in operation.Data.Select(d => d.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)))
            {
                if (_autoIncrementColumns.ContainsKey(operation.TableName))
                {
                    var table = GetTable(operation.TableName);
                    var autoIncrementColumn = _autoIncrementColumns[operation.TableName];

                    if (!data.ContainsKey(autoIncrementColumn))
                    {
                        data.Add(autoIncrementColumn, 0);
                    }

                    object nextVal = 0;
                    if (table.Count > 0)
                    {
                        nextVal = table.Select(d => d[autoIncrementColumn]).Max();
                    }

                    nextVal = ObjectMaths.Increment(nextVal);
                    data[autoIncrementColumn] = nextVal;
                }

                GetTable(operation.TableName).Add(data);

                AddAsDetail(operation.TableName, data);
                AddAsMaster(operation.TableName, data);

                yield return data;
            }
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

        private CommandResult Update(string tableName, IReadOnlyDictionary<string, object> data, SimpleExpression criteria)
        {
            int count = 0;
            foreach (var record in Find(tableName, criteria))
            {
                UpdateRecord(data, record);
                ++count;
            }
            return new CommandResult(count);
        }

        private static void UpdateRecord(IEnumerable<KeyValuePair<string, object>> data, IDictionary<string, object> record)
        {
            foreach (var kvp in data)
            {
                record[kvp.Key] = kvp.Value;
            }
        }

        private CommandResult Delete(string tableName, SimpleExpression criteria)
        {
            List<IDictionary<string, object>> deletions = Find(tableName, criteria).ToList();
            foreach (var record in deletions)
            {
                GetTable(tableName).Remove(record);
            }
            return new CommandResult(deletions.Count);
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

        private readonly IEqualityComparer<string> _keyComparer = EqualityComparer<string>.Default;

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

        public DataResult Get(GetOperation operation, IAdapterTransaction transaction)
        {
            return Get(operation);
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

        public OperationResult Execute(FunctionOperation operation)
        {
            if (!_functions.ContainsKey(operation.FunctionName)) throw new InvalidOperationException(string.Format("Function '{0}' not found.", operation.FunctionName));
            var obj = ((_functions[operation.FunctionName].Flags & FunctionFlags.PassThru) == FunctionFlags.PassThru) ?
                            _functions[operation.FunctionName].Delegate.DynamicInvoke(operation.Parameters) :
                            _functions[operation.FunctionName].Delegate.DynamicInvoke(operation.Parameters.Values.ToArray());

            var dict = obj as IDictionary<string, object>;
            if (dict != null)
            {
                return new DataResult(EnumerableEx.Once(dict));
            }

            var list = obj as IEnumerable<IDictionary<string, object>>;
            if (list != null) return new DataResult(list);

            var lists = obj as IEnumerable<IEnumerable<IDictionary<string, object>>>;

            if (lists != null) return new MultiDataResult(lists);

            if (obj is int)
            {
                return new CommandResult((int)obj);
            }

            return null;
        }
    }
}