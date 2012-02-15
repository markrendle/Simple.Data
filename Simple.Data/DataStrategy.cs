using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using Simple.Data.Commands;

namespace Simple.Data
{
    /// <summary>
    /// This class supports the Simple.Data framework internally and should not be used in your code.
    /// </summary>
    public abstract class DataStrategy : DynamicObject
    {
        private readonly ConcurrentDictionary<string, dynamic> _members = new ConcurrentDictionary<string, dynamic>();

        public abstract Adapter GetAdapter();

        /// <summary>
        /// Provides the implementation for operations that get member values. Classes derived from the <see cref="T:System.Dynamic.DynamicObject"/> class can override this method to specify dynamic behavior for operations such as getting a value for a property.
        /// </summary>
        /// <param name="binder">Provides information about the object that called the dynamic operation. The binder.Name property provides the name of the member on which the dynamic operation is performed. For example, for the Console.WriteLine(sampleObject.SampleProperty) statement, where sampleObject is an instance of the class derived from the <see cref="T:System.Dynamic.DynamicObject"/> class, binder.Name returns "SampleProperty". The binder.IgnoreCase property specifies whether the member name is case-sensitive.</param>
        /// <param name="result">The result of the get operation. For example, if the method is called for a property, you can assign the property value to <paramref name="result"/>.</param>
        /// <returns>
        /// true if the operation is successful; otherwise, false. If this method returns false, the run-time binder of the language determines the behavior. (In most cases, a run-time exception is thrown.)
        /// </returns>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return GetDynamicMember(binder, out result);
        }

        protected virtual bool GetDynamicMember(GetMemberBinder binder, out object result)
        {
            result = GetOrAddDynamicReference(binder.Name);
            return true;
        }

        private dynamic GetOrAddDynamicReference(string name)
        {
            return _members.GetOrAdd(name, CreateDynamicReference);
        }

        internal bool TryInvokeFunction(String functionName, Func<IDictionary<String,Object>> getFunctionArguments, out object result)
        {
            var adapterWithFunctions = GetAdapter() as IAdapterWithFunctions;
            if (adapterWithFunctions != null && adapterWithFunctions.IsValidFunction(functionName))
            {
                var command = new ExecuteFunctionCommand(GetDatabase(), adapterWithFunctions, functionName, getFunctionArguments());
                return command.Execute(out result);
            }
            result = null;
            return false;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            if (this.TryInvokeFunction(binder.Name, () => binder.ArgumentsToDictionary(args), out result)) return true;

            return base.TryInvokeMember(binder, args, out result);
        }
        
        public dynamic this[string name]
        {
            get { return GetOrAddDynamicReference(name); }
        }

        private ObjectReference CreateDynamicReference(string name)
        {
            return new ObjectReference(name, this);
        }

        internal DynamicTable SetMemberAsTable(ObjectReference reference)
        {
            if (reference == null) throw new ArgumentNullException("reference");
            _members.TryUpdate(reference.GetName(), new DynamicTable(reference.GetName(), this), reference);
            return (DynamicTable)_members[reference.GetName()];
        }

        internal DynamicTable SetMemberAsTable(ObjectReference reference, DynamicTable table)
        {
            if (reference == null) throw new ArgumentNullException("reference");
            _members.TryUpdate(reference.GetName(), table, reference);
            return (DynamicTable)_members[reference.GetName()];
        }

        internal DynamicSchema SetMemberAsSchema(ObjectReference reference)
        {
            if (reference == null) throw new ArgumentNullException("reference");
            _members.TryUpdate(reference.GetName(), new DynamicSchema(reference.GetName(), this), reference);
            return (DynamicSchema)_members[reference.GetName()];
        }

        internal DynamicSchema SetMemberAsSchema(ObjectReference reference, DynamicSchema schema)
        {
            if (reference == null) throw new ArgumentNullException("reference");
            _members.TryUpdate(reference.GetName(), schema, reference);
            return (DynamicSchema)_members[reference.GetName()];
        }

        protected internal abstract Database GetDatabase();

        /// <summary>
        ///  Finds data from the specified "table".
        ///  </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="criteria">The criteria. This may be <c>null</c>, in which case all records should be returned.</param>
        /// <returns>The list of records matching the criteria. If no records are found, return an empty list.</returns>
        internal abstract IEnumerable<IDictionary<string, object>> Find(string tableName, SimpleExpression criteria);

        /// <summary>
        ///  Inserts a record into the specified "table".
        ///  </summary><param name="tableName">Name of the table.</param><param name="data">The values to insert.</param><returns>If possible, return the newly inserted row, including any automatically-set values such as primary keys or timestamps.</returns>
        internal abstract IEnumerable<IDictionary<string, object>> Insert(string tableName, IEnumerable<IDictionary<string, object>> enumerable, ErrorCallback onError, bool resultRequired);

        /// <summary>
        ///  Inserts many records into the specified "table".
        ///  </summary><param name="tableName">Name of the table.</param><param name="data">The values to insert.</param><returns>If possible, return the newly inserted row, including any automatically-set values such as primary keys or timestamps.</returns>
        internal abstract IDictionary<string, object> Insert(string tableName, IDictionary<string, object> data, bool resultRequired);

        /// <summary>
        ///  Updates the specified "table" according to specified criteria.
        ///  </summary><param name="tableName">Name of the table.</param><param name="data">The new values.</param><param name="criteria">The expression to use as criteria for the update operation.</param><returns>The number of records affected by the update operation.</returns>
        internal abstract int Update(string tableName, IDictionary<string, object> data, SimpleExpression criteria);

        /// <summary>
        ///  Deletes from the specified table.
        ///  </summary><param name="tableName">Name of the table.</param><param name="criteria">The expression to use as criteria for the delete operation.</param><returns>The number of records which were deleted.</returns>
        internal abstract int Delete(string tableName, SimpleExpression criteria);

        internal abstract IDictionary<string, object> FindOne(string getQualifiedName, SimpleExpression criteriaExpression);
        internal abstract int UpdateMany(string tableName, IList<IDictionary<string, object>> dataList);

        internal bool IsExpressionFunction(string name, object[] args)
        {
            return GetAdapter().IsExpressionFunction(name, args);
        }

        internal abstract int UpdateMany(string tableName, IList<IDictionary<string, object>> dataList, IEnumerable<string> criteriaFieldNames);

        internal abstract int UpdateMany(string tableName, IList<IDictionary<string, object>> newValuesList,
                                         IList<IDictionary<string, object>> originalValuesList);

        public abstract int Update(string tableName, IDictionary<string, object> newValuesDict, IDictionary<string, object> originalValuesDict);

        protected static SimpleExpression CreateCriteriaFromOriginalValues(string tableName, IDictionary<string, object> newValuesDict, IDictionary<string, object> originalValuesDict)
        {
            var criteriaValues = originalValuesDict
                .Where(originalKvp => newValuesDict.ContainsKey(originalKvp.Key) && !(Equals(newValuesDict[originalKvp.Key], originalKvp.Value)));

            return ExpressionHelper.CriteriaDictionaryToExpression(tableName, criteriaValues);
        }

        protected static Dictionary<string, object> CreateChangedValuesDict(IEnumerable<KeyValuePair<string, object>> newValuesDict, IDictionary<string, object> originalValuesDict)
        {
            var changedValuesDict =
                newValuesDict.Where(
                    kvp =>
                    (!originalValuesDict.ContainsKey(kvp.Key)) || !(Equals(kvp.Value, originalValuesDict[kvp.Key])))
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            return changedValuesDict;
        }

        public abstract IEnumerable<IDictionary<string,object>> UpsertMany(string tableName, IList<IDictionary<string, object>> list, IEnumerable<string> keyFieldNames);

        public abstract IDictionary<string,object> Upsert(string tableName, IDictionary<string, object> dict, SimpleExpression criteriaExpression, bool isResultRequired);

        public abstract IEnumerable<IDictionary<string,object>> UpsertMany(string tableName, IList<IDictionary<string, object>> list);
    }
}
