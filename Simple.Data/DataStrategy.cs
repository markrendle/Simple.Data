using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using Simple.Data.Commands;

namespace Simple.Data
{
    using System.Reflection;

    /// <summary>
    /// This class supports the Simple.Data framework internally and should not be used in your code.
    /// </summary>
    public abstract class DataStrategy : DynamicObject, ICloneable
    {
        private readonly ConcurrentDictionary<string, dynamic> _members = new ConcurrentDictionary<string, dynamic>();

        protected DataStrategy() {}

        protected DataStrategy(DataStrategy copy)
        {
            _members = copy._members;
        }

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

        internal bool TryInvokeFunction(String functionName, Func<IDictionary<String, Object>> getFunctionArguments,
                                        out object result)
        {
            var adapterWithFunctions = GetAdapter() as IAdapterWithFunctions;
            if (adapterWithFunctions != null && adapterWithFunctions.IsValidFunction(functionName))
            {
                var command = new ExecuteFunctionCommand(GetDatabase(), adapterWithFunctions, functionName,
                                                         getFunctionArguments());
                return ExecuteFunction(out result, command);
            }
            result = null;
            return false;
        }

        protected internal abstract bool ExecuteFunction(out object result, ExecuteFunctionCommand command);

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            if (TryInvokeFunction(binder.Name, () => binder.ArgumentsToDictionary(args), out result)) return true;

            if (new AdapterMethodDynamicInvoker(GetAdapter()).TryInvokeMember(binder, args, out result)) return true;

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
            return (DynamicTable) _members[reference.GetName()];
        }

        internal DynamicTable SetMemberAsTable(ObjectReference reference, DynamicTable table)
        {
            if (reference == null) throw new ArgumentNullException("reference");
            _members.TryUpdate(reference.GetName(), table, reference);
            return (DynamicTable) _members[reference.GetName()];
        }

        internal DynamicSchema SetMemberAsSchema(ObjectReference reference)
        {
            if (reference == null) throw new ArgumentNullException("reference");
            _members.TryUpdate(reference.GetName(), new DynamicSchema(reference.GetName(), this), reference);
            return (DynamicSchema) _members[reference.GetName()];
        }

        internal DynamicSchema SetMemberAsSchema(ObjectReference reference, DynamicSchema schema)
        {
            if (reference == null) throw new ArgumentNullException("reference");
            _members.TryUpdate(reference.GetName(), schema, reference);
            return (DynamicSchema) _members[reference.GetName()];
        }

        protected internal abstract Database GetDatabase();

        internal bool IsExpressionFunction(string name, object[] args)
        {
            return GetAdapter().IsExpressionFunction(name, args);
        }

        internal abstract RunStrategy Run { get; }

        object ICloneable.Clone()
        {
            return Clone();
        }

        protected internal abstract DataStrategy Clone();

        public dynamic WithOptions(OptionsBase options)
        {
            return new DataStrategyWithOptions(this, options);
        }
        
        public virtual dynamic ClearOptions()
        {
            return this;
        }
    }
}
