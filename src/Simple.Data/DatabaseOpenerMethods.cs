namespace Simple.Data
{
    using System;

    internal class DatabaseOpenerMethods
    {
        private Func<Database> _openDefault;
        private Func<string, Database> _openFile;
        private Func<string, Database> _openConnection;
        private Func<string, string, Database> _openConnectionWithProvider;
        private Func<string, Database> _openNamedConnection;
        private Func<string, object, Database> _open;

        public Func<Database> OpenDefaultImpl
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

        public Func<string, Database> OpenNamedConnectionImpl
        {
            get { return _openNamedConnection ?? DatabaseOpener.OpenNamedConnectionMethod; }
        }

        public Func<string, object, Database> OpenImpl
        {
            get { return _open ?? DatabaseOpener.OpenMethod; }
        }

        public void UseMockDatabase(Database database)
        {
            _openDefault = () => database;
            _openFile = _openConnection = _openNamedConnection = ignore => database;
            _open = (ignore1, ignore2) => database;
            _openConnectionWithProvider = (ignore1, ignore2) => database;
        }

        public void UseMockAdapter(Adapter adapter)
        {
            _openDefault = () => new Database(adapter);
            _openFile = _openConnection = _openNamedConnection = ignore => new Database(adapter);
            _open = (ignore1, ignore2) => new Database(adapter);
            _openConnectionWithProvider = (ignore1, ignore2) => new Database(adapter);
        }

        public void UseMockDatabase(Func<Database> databaseCreator)
        {
            _openDefault = databaseCreator;
            _openFile = _openConnection = _openNamedConnection = ignore => databaseCreator();
            _open = (ignore1, ignore2) => databaseCreator();
            _openConnectionWithProvider = (ignore1, ignore2) => databaseCreator();
        }

        public void UseMockAdapter(Func<Adapter> adapterCreator)
        {
            _openDefault = () => new Database(adapterCreator());
            _openFile = _openConnection = _openNamedConnection = ignore => new Database(adapterCreator());
            _open = (ignore1, ignore2) => new Database(adapterCreator());
            _openConnectionWithProvider = (ignore1, ignore2) => new Database(adapterCreator());
        }

        public void StopUsingMock()
        {
            _openDefault = null;
            _openFile = _openConnection = _openNamedConnection = null;
            _open = null;
            _openConnectionWithProvider = null;
        }
    }
}