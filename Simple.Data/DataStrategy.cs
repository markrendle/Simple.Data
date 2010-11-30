using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace Simple.Data
{
    /// <summary>
    /// This class supports the Simple.Data framework internally and should not be used in your code.
    /// </summary>
    public abstract class DataStrategy : DynamicObject
    {
        private readonly ConcurrentDictionary<string, dynamic> _members = new ConcurrentDictionary<string, dynamic>();

        internal abstract Adapter Adapter { get; }

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
            result = _members.GetOrAdd(binder.Name, CreateDynamicReference);
            return true;
        }

        private DynamicReference CreateDynamicReference(string name)
        {
            return new DynamicReference(name, this);
        }

        internal DynamicTable SetMemberAsTable(DynamicReference reference)
        {
            if (reference == null) throw new ArgumentNullException("reference");
            _members.TryUpdate(reference.GetName(), new DynamicTable(reference.GetName(), this), reference);
            return (DynamicTable)_members[reference.GetName()];
        }

        internal DynamicSchema SetMemberAsSchema(DynamicReference reference)
        {
            if (reference == null) throw new ArgumentNullException("reference");
            _members.TryUpdate(reference.GetName(), new DynamicSchema(reference.GetName(), this), reference);
            return (DynamicSchema)_members[reference.GetName()];
        }

        /// <summary>
        ///  Finds data from the specified "table".
        ///  </summary>
        /// <param name="target"></param>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="criteria">The criteria. This may be <c>null</c>, in which case all records should be returned.</param>
        /// <returns>The list of records matching the criteria. If no records are found, return an empty list.</returns>
        public IEnumerable<IDictionary<string, object>> Find(string tableName, SimpleExpression criteria)
        {
            var transaction = this as SimpleTransaction;
            if (transaction != null)
            {
                return ((IAdapterWithTransactions)Adapter).Find(tableName, criteria, transaction.AdapterTransaction);
            }
            return Adapter.Find(tableName, criteria);
        }

        /// <summary>
        ///  Inserts a record into the specified "table".
        ///  </summary><param name="tableName">Name of the table.</param><param name="data">The values to insert.</param><returns>If possible, return the newly inserted row, including any automatically-set values such as primary keys or timestamps.</returns>
        public IDictionary<string, object> Insert(string tableName, IDictionary<string, object> data)
        {
            var transaction = this as SimpleTransaction;
            if (transaction != null)
            {
                return ((IAdapterWithTransactions)this).Insert(tableName, data, transaction.AdapterTransaction);
            }
            return Adapter.Insert(tableName, data);
        }

        /// <summary>
        ///  Updates the specified "table" according to specified criteria.
        ///  </summary><param name="tableName">Name of the table.</param><param name="data">The new values.</param><param name="criteria">The expression to use as criteria for the update operation.</param><returns>The number of records affected by the update operation.</returns>
        public int Update(string tableName, IDictionary<string, object> data, SimpleExpression criteria)
        {
            var transaction = this as SimpleTransaction;
            if (transaction != null)
            {
                return ((IAdapterWithTransactions)this).Update(tableName, data, criteria, transaction.AdapterTransaction);
            }
            return Adapter.Update(tableName, data, criteria);
        }

        /// <summary>
        ///  Deletes from the specified table.
        ///  </summary><param name="tableName">Name of the table.</param><param name="criteria">The expression to use as criteria for the delete operation.</param><returns>The number of records which were deleted.</returns>
        public int Delete(string tableName, SimpleExpression criteria)
        {
            var transaction = this as SimpleTransaction;
            if (transaction != null)
            {
                return ((IAdapterWithTransactions)this).Delete(tableName, criteria, transaction.AdapterTransaction);
            }
            return Adapter.Delete(tableName, criteria);
        }

    }
}
