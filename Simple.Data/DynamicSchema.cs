using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace Simple.Data
{
    using Commands;

    public class DynamicSchema : DynamicObject
    {
        private readonly string _name;
        private readonly DataStrategy _dataStrategy;

        private readonly ConcurrentDictionary<string, dynamic> _tables = new ConcurrentDictionary<string, dynamic>();

        public DynamicSchema(string name, DataStrategy dataStrategy)
        {
            _name = name;
            _dataStrategy = dataStrategy;
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
            return GetDynamicTable(binder.Name, out result);
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            var adapterWithFunctions = _dataStrategy.GetAdapter() as IAdapterWithFunctions;
            if (adapterWithFunctions != null && adapterWithFunctions.IsValidFunction(string.Format("{0}.{1}", _name, binder.Name)))
            {
                var command = new ExecuteFunctionCommand(_dataStrategy.GetDatabase(), adapterWithFunctions, string.Format("{0}.{1}", _name, binder.Name),
                                                         binder.ArgumentsToDictionary(args));
                return command.Execute(out result);
            }
            return base.TryInvokeMember(binder, args, out result);
        }

        public DynamicTable this[string name]
        {
            get { return GetTable(name); }
        }

        internal DynamicTable GetTable(string name)
        {
            dynamic table;
            GetDynamicTable(name, out table);
            return table;
        }

        internal string GetName()
        {
            return _name;
        }

        private bool GetDynamicTable(string name, out object result)
        {
            result = _tables.GetOrAdd(name, CreateDynamicTable);
            return true;
        }

        private DynamicTable CreateDynamicTable(string name)
        {
            return new DynamicTable(name, _dataStrategy, this);
        }
    }
}
