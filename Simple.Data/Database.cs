using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading;
using Simple.Data.Ado;
using Simple.Data.Properties;

namespace Simple.Data
{
    /// <summary>
    /// The entry class for Simple.Data. Provides static methods for opening databases,
    /// and implements runtime dynamic functionality for resolving database-level objects.
    /// </summary>
    public class Database : DynamicObject
    {
        private readonly IAdapter _adapter;
        private static readonly Lazy<dynamic> _default = new Lazy<dynamic>(Open, LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// Initializes a new instance of the <see cref="Database"/> class.
        /// </summary>
        /// <param name="adapter">The adapter to use for data access.</param>
        internal Database(IAdapter adapter)
        {
            _adapter = adapter;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Database"/> class.
        /// </summary>
        /// <param name="connectionProvider">The connection provider to use with <see cref="AdoAdapter"/> for data access.</param>
        internal Database(IConnectionProvider connectionProvider)
        {
            _adapter = new AdoAdapter(this, connectionProvider);
        }

        /// <summary>
        /// Gets the adapter being used by the <see cref="Database"/> instance.
        /// </summary>
        /// <value>The adapter.</value>
        public IAdapter Adapter
        {
            get { return _adapter; }
        }

        /// <summary>
        /// Gets a default instance of the Database. This connects to an ADO.NET data source
        /// specified in the 'Simple.Data.Properties.Settings.ConnectionString' config ConnectionStrings setting.
        /// </summary>
        /// <value>The default database.</value>
        public static dynamic Default
        {
            get { return _default.Value; }
        }

        /// <summary>
        /// Opens an instance of <see cref="Database"/> which connects to an ADO.NET data source
        /// specified in the 'Simple.Data.Properties.Settings.ConnectionString' config ConnectionStrings setting.
        /// </summary>
        /// <returns>A <see cref="Database"/> object as a dynamic type.</returns>
        public static dynamic Open()
        {
            return DatabaseOpener.OpenDefault();
        }

        /// <summary>
        /// Opens an instance of <see cref="Database"/> which connects to an ADO.NET data source
        /// specified in the connectionString parameter.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <returns>A <see cref="Database"/> object as a dynamic type.</returns>
        public static dynamic OpenConnection(string connectionString)
        {
            return DatabaseOpener.OpenConnection(connectionString);
        }

        /// <summary>
        /// Opens an instance of <see cref="Database"/> which connects to the SQL CE database
        /// specified in the filename parameter.
        /// </summary>
        /// <param name="filename">The name of the SQL CE database file.</param>
        /// <returns>A <see cref="Database"/> object as a dynamic type.</returns>
        public static dynamic OpenFile(string filename)
        {
            return DatabaseOpener.OpenFile(filename);
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
            return base.TryGetMember(binder, out result)
                   || NewDynamicTable(binder, out result);
        }

        private bool NewDynamicTable(GetMemberBinder binder, out object result)
        {
            result = new DynamicTable(binder.Name, this);
            return true;
        }
    }
}