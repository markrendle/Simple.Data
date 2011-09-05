namespace Simple.Data
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

        public dynamic OpenDefault()
        {
            return OpenMethods.OpenDefaultImpl();
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

        public dynamic Open(string adapterName, object settings)
        {
            return OpenMethods.OpenImpl(adapterName, settings);
        }

        public dynamic OpenNamedConnection(string connectionName)
        {
            return OpenMethods.OpenNamedConnectionImpl(connectionName);
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

        internal static Database OpenDefaultMethod()
        {
            return new Database(AdapterFactory.Create("Ado", new { ConnectionName = "Simple.Data.Properties.Settings.DefaultConnectionString" }));
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

        internal static Database OpenNamedConnectionMethod(string connectionName)
        {
            return new Database(AdapterFactory.Create("Ado", new { ConnectionName = connectionName }));
        }

        internal static Database OpenMethod(string adapterName, object settings)
        {
            return new Database(AdapterFactory.Create(adapterName, settings));
        }
    }
}
