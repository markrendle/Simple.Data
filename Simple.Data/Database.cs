using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Simple.Data
{
    /// <summary>
    /// The entry class for Simple.Data. Provides static methods for opening databases,
    /// and implements runtime dynamic functionality for resolving database-level objects.
    /// </summary>
    public partial class Database : DynamicObject
    {
        private static readonly IDatabaseOpener DatabaseOpener;
        private static readonly Lazy<dynamic> LazyDefault;
        private readonly ConcurrentDictionary<string, dynamic> _members = new ConcurrentDictionary<string, dynamic>();
        private readonly Adapter _adapter;

        static Database()
        {
            DatabaseOpener = new DatabaseOpener();
            LazyDefault = new Lazy<dynamic>(DatabaseOpener.OpenDefault, LazyThreadSafetyMode.ExecutionAndPublication);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Database"/> class.
        /// </summary>
        /// <param name="adapter">The adapter to use for data access.</param>
        internal Database(Adapter adapter)
        {
            _adapter = adapter;
        }

        /// <summary>
        /// Gets the adapter being used by the <see cref="Database"/> instance.
        /// </summary>
        /// <value>The adapter.</value>
        internal Adapter Adapter
        {
            get { return _adapter; }
        }

        public static IDatabaseOpener Opener
        {
            get { return DatabaseOpener; }
        }

        /// <summary>
        /// Gets a default instance of the Database. This connects to an ADO.NET data source
        /// specified in the 'Simple.Data.Properties.Settings.ConnectionString' config ConnectionStrings setting.
        /// </summary>
        /// <value>The default database.</value>
        public static dynamic Default
        {
            get { return LazyDefault.Value; }
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
            return GetDynamicMember(binder, out result);
        }

        private bool GetDynamicMember(GetMemberBinder binder, out object result)
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
            return (DynamicTable) _members[reference.GetName()];
        }

        internal DynamicSchema SetMemberAsSchema(DynamicReference reference)
        {
            if (reference == null) throw new ArgumentNullException("reference");
            _members.TryUpdate(reference.GetName(), new DynamicSchema(reference.GetName(), this), reference);
            return (DynamicSchema) _members[reference.GetName()];
        }
    }
}