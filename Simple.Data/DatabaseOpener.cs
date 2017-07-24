namespace Shitty.Data
{
    using System;
    using System.Threading;

    internal class DatabaseOpener : IDatabaseOpener
    {
        private static readonly IAdapterFactory AdapterFactory = new CachingAdapterFactory();
        private static readonly ThreadLocal<DatabaseOpenerMethods> LocalOpenMethods = new ThreadLocal<DatabaseOpenerMethods>(() => new DatabaseOpenerMethods());

        protected static DatabaseOpenerMethods OpenMethods
        {
            get { return LocalOpenMethods.Value; }
        }

        public dynamic OpenDefault(string schemaName = null)
        {
            return OpenMethods.OpenDefaultImpl(schemaName);
        }

        public dynamic OpenFile(string filename)
        {
            return OpenMethods.OpenFileImpl(filename);
        }

        public dynamic OpenConnection(string connectionString)
        {
            return OpenMethods.OpenConnectionImpl(connectionString);
        }

        public dynamic OpenConnection(string connectionString, string providerName)
        {
            return OpenMethods.OpenConnectionWithProviderImpl(connectionString, providerName);
        }

        public dynamic OpenConnection(string connectionString, string providerName, string schemaName)
        {
            return OpenMethods.OpenConnectionWithProviderAndSchemaImpl(connectionString, providerName, schemaName);
        }

        public dynamic Open(string adapterName, object settings)
        {
            return OpenMethods.OpenImpl(adapterName, settings);
        }

        public dynamic OpenNamedConnection(string connectionName)
        {
            return OpenMethods.OpenNamedConnectionImpl(connectionName);
        }

        public dynamic OpenNamedConnection(string connectionName, string schemaName)
        {
            return OpenMethods.OpenNamedConnectionAndSchemaImpl(connectionName, schemaName);
        }

        public void ClearAdapterCache()
        {
            ((CachingAdapterFactory) AdapterFactory).Reset();
        }

        public static void UseMockDatabase(Database database)
        {
            OpenMethods.UseMockDatabase(database);
        }

        public static void UseMockAdapter(Adapter adapter)
        {
            OpenMethods.UseMockAdapter(adapter);
        }

        public static void UseMockDatabase(Func<Database> databaseCreator)
        {
            OpenMethods.UseMockDatabase(databaseCreator);
        }

        public static void UseMockAdapter(Func<Adapter> adapterCreator)
        {
            OpenMethods.UseMockAdapter(adapterCreator);
        }

        internal static Database OpenDefaultMethod(string schemaName = null)
        {
            return new Database(AdapterFactory.Create("Ado", new { ConnectionName = "Simple.Data.Properties.Settings.DefaultConnectionString", SchemaName = schemaName }));
        }

        internal static Database OpenFileMethod(string filename)
        {
            return new Database(AdapterFactory.Create("Ado", new { Filename = filename }));
        }

        internal static Database OpenConnectionMethod(string connectionString)
        {
            return new Database(AdapterFactory.Create("Ado", new { ConnectionString = connectionString }));
        }

        internal static Database OpenConnectionMethod(string connectionString, string providerName)
        {
            return new Database(AdapterFactory.Create("Ado", new { ConnectionString = connectionString, ProviderName = providerName }));
        }

        internal static Database OpenConnectionAndSchemaMethod(string connectionString, string providerName, string schemaName)
        {
            return new Database(AdapterFactory.Create("Ado", new { ConnectionString = connectionString, ProviderName = providerName, SchemaName = schemaName }));
        }

        internal static Database OpenNamedConnectionMethod(string connectionName)
        {
            return new Database(AdapterFactory.Create("Ado", new { ConnectionName = connectionName }));
        }

        internal static Database OpenNamedConnectionAndSchemaMethod(string connectionName, string schemaName)
        {
            return new Database(AdapterFactory.Create("Ado", new { ConnectionName = connectionName, SchemaName = schemaName }));
        }

        internal static Database OpenMethod(string adapterName, object settings)
        {
            return new Database(AdapterFactory.Create(adapterName, settings));
        }

        public static void StopUsingMock()
        {
            OpenMethods.StopUsingMock();
        }
    }
}
