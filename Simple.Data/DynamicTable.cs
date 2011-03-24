using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Dynamic;
using Simple.Data.Commands;

namespace Simple.Data
{
    /// <summary>
    /// Represents a table in a database, or the nearest equivalent in other data stores.
    /// </summary>
    public class DynamicTable : DynamicObject
    {
        private readonly string _tableName;
        private readonly DynamicSchema _schema;
        private readonly DataStrategy _dataStrategy;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicTable"/> class.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="dataStrategy">The database which owns the table.</param>
        internal DynamicTable(string tableName, DataStrategy dataStrategy)
            : this(tableName, dataStrategy, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicTable"/> class.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="dataStrategy">The database which owns the table.</param>
        /// <param name="schema">The schema to which the table belongs.</param>
        internal DynamicTable(string tableName, DataStrategy dataStrategy, DynamicSchema schema)
        {
            _tableName = tableName;
            _schema = schema;
            _dataStrategy = dataStrategy;
        }

        /// <summary>
        /// Provides the implementation for operations that invoke a member. Classes derived from the <see cref="T:System.Dynamic.DynamicObject"/> class can override this method to specify dynamic behavior for operations such as calling a method.
        /// </summary>
        /// <param name="binder">Provides information about the dynamic operation. The binder.Name property provides the name of the member on which the dynamic operation is performed. For example, for the statement sampleObject.SampleMethod(100), where sampleObject is an instance of the class derived from the <see cref="T:System.Dynamic.DynamicObject"/> class, binder.Name returns "SampleMethod". The binder.IgnoreCase property specifies whether the member name is case-sensitive.</param>
        /// <param name="args">The arguments that are passed to the object member during the invoke operation. For example, for the statement sampleObject.SampleMethod(100), where sampleObject is derived from the <see cref="T:System.Dynamic.DynamicObject"/> class, <paramref name="args[0]"/> is equal to 100.</param>
        /// <param name="result">The result of the member invocation.</param>
        /// <returns>
        /// true if the operation is successful; otherwise, false. If this method returns false, the run-time binder of the language determines the behavior. (In most cases, a language-specific run-time exception is thrown.)
        /// </returns>
        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            var command = CommandFactory.GetCommandFor(binder.Name);
            if (command != null)
            {
                result = command.Execute(_dataStrategy, this, binder, args);
                return true;
            }
            return base.TryInvokeMember(binder, args, out result);
        }

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
            if (binder.Name == "All")
            {
                Trace.WriteLine("The dynamic 'All' property is deprecated; use the 'All()' method instead.");
                result = GetAll().ToList();
                return true;
            }
            result = new DynamicReference(binder.Name, new DynamicReference(_tableName, _dataStrategy));
            return true;
        }

        public DynamicReference this[string name]
        {
            get { return new DynamicReference(name, new DynamicReference(_tableName, _dataStrategy)); }
        }

        /// <summary>
        /// Inserts the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public void Insert(dynamic entity)
        {
            var dictionary = entity as IDictionary<string, object>;
            if (dictionary != null)
            {
                _dataStrategy.Insert(_tableName, dictionary);
            }
        }

        internal string GetName()
        {
            return _tableName;
        }

        internal string GetQualifiedName()
        {
            return _schema != null ? _schema.GetName() + "." + _tableName : _tableName;
        }

        private IEnumerable<dynamic> GetAll()
        {
            return _dataStrategy.Find(_tableName, null).Select(dict => new SimpleRecord(dict, _tableName, _dataStrategy));
        }
    }
}
