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
    public partial class Database : DataStrategy
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
        internal override Adapter Adapter
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
    }
}