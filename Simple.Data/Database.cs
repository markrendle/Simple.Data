using System;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Simple.Data
{
    using System.Configuration;
    using System.Data;
    using System.Diagnostics;

    /// <summary>
    /// The entry class for Simple.Data. Provides static methods for opening databases,
    /// and implements runtime dynamic functionality for resolving database-level objects.
    /// </summary>
    public sealed partial class Database : DataStrategy
    {
        private static SimpleDataConfigurationSection _configuration;

        private static readonly IDatabaseOpener DatabaseOpener;
        private static IPluralizer _pluralizer;
        private readonly Adapter _adapter;
        private readonly DatabaseRunner _databaseRunner;

        static Database()
        {
            DatabaseOpener = new DatabaseOpener();
            LoadTraceLevelFromConfig();
        }

        private static void LoadTraceLevelFromConfig()
        {
            _configuration =
                (SimpleDataConfigurationSection) ConfigurationManager.GetSection("simpleData/simpleDataConfiguration");
            if (_configuration != null)
            {
                Trace.TraceWarning("SimpleDataConfiguration section is obsolete; use system.diagnostics switches instead.");
                TraceLevel = _configuration.TraceLevel;
            }
            else
            {
                var traceSwitch = new TraceSwitch("Simple.Data", "", TraceLevel.Info.ToString());
                TraceLevel = traceSwitch.Level;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Database"/> class.
        /// </summary>
        /// <param name="adapter">The adapter to use for data access.</param>
        internal Database(Adapter adapter)
        {
            _adapter = adapter;
            _databaseRunner = new DatabaseRunner(_adapter);
        }

        private Database(Database copy) : base(copy)
        {
            _adapter = copy._adapter;
            _databaseRunner = copy._databaseRunner;
        }

        public override Adapter GetAdapter()
        {
            return _adapter;
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
            get { return DatabaseOpener.OpenDefault(); }
        }

        public SimpleTransaction BeginTransaction()
        {
            return SimpleTransaction.Begin(this);
        }

        public SimpleTransaction BeginTransaction(string name)
        {
            return SimpleTransaction.Begin(this, name);
        }

        public SimpleTransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            return SimpleTransaction.Begin(this, isolationLevel);
        }

        protected internal override bool ExecuteFunction(out object result, Commands.ExecuteFunctionCommand command)
        {
            return command.Execute(out result);
        }

        protected internal override Database GetDatabase()
        {
            return this;
        }

        public static IPluralizer GetPluralizer()
        {
            return _pluralizer;
        }

        public static void SetPluralizer(IPluralizer pluralizer)
        {
            _pluralizer = pluralizer;
            Extensions.StringExtensions.SetPluralizer(pluralizer);
        }

        public static void ClearAdapterCache()
        {
            DatabaseOpener.ClearAdapterCache();
        }

        public static void UseMockAdapter(Adapter mockAdapter)
        {
            Data.DatabaseOpener.UseMockAdapter(mockAdapter);
        }

        public static void UseMockAdapter(Func<Adapter> mockAdapterCreator)
        {
            Data.DatabaseOpener.UseMockAdapter(mockAdapterCreator());
        }

        public static void StopUsingMockAdapter()
        {
            Data.DatabaseOpener.StopUsingMock();
        }

        private static TraceLevel? _traceLevel;
        public static TraceLevel TraceLevel
        {
            get { return _traceLevel ?? _configuration.TraceLevel; }
            set { _traceLevel = value; }
        }

        internal override RunStrategy Run
        {
            get { return _databaseRunner; }
        }

        protected internal override DataStrategy Clone()
        {
            return new Database(this);
        }
    }
}