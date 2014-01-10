namespace Simple.Data
{
    using System;

    internal class DatabaseOpenerMethods
    {
        private Func<string, Database> _openDefault;
        private Func<string, Database> _openFile;
        private Func<string, Database> _openConnection;
        private Func<string, string, Database> _openConnectionWithProvider;
        private Func<string, string, string, Database> _openConnectionWithProviderAndSchema;
        private Func<string, Database> _openNamedConnection;
        private Func<string, string, Database> _openNamedConnectionAndSchema;
        private Func<string, object, Database> _open;

        public Func<string, Database> OpenDefaultImpl
        {
            get { return _openDefault ?? DatabaseOpener.OpenDefaultMethod; }
        }

        public Func<string, Database> OpenFileImpl
        {
            get { return _openFile ?? DatabaseOpener.OpenFileMethod; }
        }

        public Func<string, Database> OpenConnectionImpl
        {
            get { return _openConnection ?? DatabaseOpener.OpenConnectionMethod; }
        }

        public Func<string, string, Database> OpenConnectionWithProviderImpl
        {
            get { return _openConnectionWithProvider ?? DatabaseOpener.OpenConnectionMethod; }
        }

        public Func<string, string, string, Database> OpenConnectionWithProviderAndSchemaImpl
        {
            get { return _openConnectionWithProviderAndSchema ?? DatabaseOpener.OpenConnectionAndSchemaMethod; }
        }

        public Func<string, Database> OpenNamedConnectionImpl
        {
            get { return _openNamedConnection ?? DatabaseOpener.OpenNamedConnectionMethod; }
        }

        public Func<string, string, Database> OpenNamedConnectionAndSchemaImpl
        {
            get { return _openNamedConnectionAndSchema ?? DatabaseOpener.OpenNamedConnectionAndSchemaMethod; }
        }

        public Func<string, object, Database> OpenImpl
        {
            get { return _open ?? DatabaseOpener.OpenMethod; }
        }

        public void UseMockDatabase(Database database)
        {
            _openDefault = _openFile = _openConnection = _openNamedConnection = ignore => database;
            _open = (ignore1, ignore2) => database;
            _openNamedConnectionAndSchema = _openConnectionWithProvider = (ignore1, ignore2) => database;
            _openConnectionWithProviderAndSchema = (ignore1, ignore2, ignore3) => database;
        }

        public void UseMockAdapter(Adapter adapter)
        {
            _openDefault = _openFile = _openConnection = _openNamedConnection = ignore => new Database(adapter);
            _open = (ignore1, ignore2) => new Database(adapter);
            _openNamedConnectionAndSchema = _openConnectionWithProvider = (ignore1, ignore2) => new Database(adapter);
            _openConnectionWithProviderAndSchema = (ignore1, ignore2, ignore3) => new Database(adapter);
        }

        public void UseMockDatabase(Func<Database> databaseCreator)
        {
            _openDefault = _openFile = _openConnection = _openNamedConnection = ignore => databaseCreator();
            _open = (ignore1, ignore2) => databaseCreator();
            _openNamedConnectionAndSchema = _openConnectionWithProvider = (ignore1, ignore2) => databaseCreator();
            _openConnectionWithProviderAndSchema = (ignore1, ignore2, ignore3) => databaseCreator();
        }

        public void UseMockAdapter(Func<Adapter> adapterCreator)
        {
            _openDefault = _openFile = _openConnection = _openNamedConnection = ignore => new Database(adapterCreator());
            _open = (ignore1, ignore2) => new Database(adapterCreator());
            _openNamedConnectionAndSchema = _openConnectionWithProvider = (ignore1, ignore2) => new Database(adapterCreator());
            _openConnectionWithProviderAndSchema = (ignore1, ignore2, ignore3) => new Database(adapterCreator());
        }

        public void StopUsingMock()
        {
            _openDefault = _openFile = _openConnection = _openNamedConnection = null;
            _open = null;
            _openNamedConnectionAndSchema = _openConnectionWithProvider = null;
            _openConnectionWithProviderAndSchema = null;
        }
    }
}