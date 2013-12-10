using System;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using Simple.Data.Commands;
using Simple.Data.Extensions;

namespace Simple.Data
{
    /// <summary>
    /// The entry class for Simple.Data. Provides static methods for opening databases,
    /// and implements runtime dynamic functionality for resolving database-level objects.
    /// </summary>
    public sealed partial class Database : DataStrategy
    {
        private static SimpleDataConfigurationSection _configuration;

        private static readonly IDatabaseOpener DatabaseOpener;
        private static IPluralizer _pluralizer;
        private static bool _loadedConfig;
        private readonly Adapter _adapter;
        private readonly DatabaseRunner _databaseRunner;

        static Database()
        {
            DatabaseOpener = new DatabaseOpener();
        }

        private static void EnsureLoadedTraceLevelFromConfig()
        {
            if (_loadedConfig)
                return;
            _loadedConfig = true;
            _configuration = (SimpleDataConfigurationSection)ConfigurationManager.GetSection("simpleData/simpleDataConfiguration");
            if (_configuration != null)
            {
                SimpleDataTraceSources.TraceSource.TraceEvent(TraceEventType.Warning, SimpleDataTraceSources.ObsoleteWarningMessageId,
                    "SimpleDataConfiguration section is obsolete; use system.diagnostics switches instead.");
                SimpleDataTraceSources.TraceSource.Switch.Level = TraceLevelToSourceLevels(_configuration.TraceLevel);
            }
        }

        private static SourceLevels TraceLevelToSourceLevels(TraceLevel traceLevel)
        {
            switch (traceLevel)
            {
                case TraceLevel.Off:
                    return SourceLevels.Off;
                case TraceLevel.Error:
                    return SourceLevels.Error;
                case TraceLevel.Warning:
                    return SourceLevels.Warning;
                case TraceLevel.Info:
                    return SourceLevels.Information;
                case TraceLevel.Verbose:
                    return SourceLevels.Verbose;
                default:
                    throw new ArgumentOutOfRangeException("traceLevel");
            }
        }

        private static TraceLevel SourceLevelsToTraceLevel(SourceLevels sourceLevels)
        {
            switch (sourceLevels)
            {
                case SourceLevels.Off:
                    return TraceLevel.Off;
                case SourceLevels.Critical:
                case SourceLevels.Error:
                    return TraceLevel.Error;
                case SourceLevels.Warning:
                    return TraceLevel.Warning;
                case SourceLevels.Information:
                    return TraceLevel.Info;
                case SourceLevels.Verbose:
                case SourceLevels.All:
                    return TraceLevel.Verbose;
                default:
                    return TraceLevel.Off; // happens if SourceLevel is assigned and TraceLevel is read, which is very unlikely
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

        protected internal override bool ExecuteFunction(out object result, ExecuteFunctionCommand command)
        {
            return command.Execute(out result);
        }

        protected internal override DataStrategy GetDatabase()
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
            StringExtensions.SetPluralizer(pluralizer);
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

        [Obsolete("Use SimpleDataTraceSources.TraceSource.Switch.Level instead.")]
        public static TraceLevel TraceLevel
        {
            get
            {
                EnsureLoadedTraceLevelFromConfig();
                return SourceLevelsToTraceLevel(SimpleDataTraceSources.TraceSource.Switch.Level);
            }
            set { SimpleDataTraceSources.TraceSource.Switch.Level = TraceLevelToSourceLevels(value); }
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