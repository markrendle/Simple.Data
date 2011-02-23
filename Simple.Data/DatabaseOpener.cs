using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Simple.Data.Properties;

namespace Simple.Data
{
    public interface IDatabaseOpener
    {
        dynamic OpenDefault();
        dynamic OpenFile(string filename);
        dynamic OpenConnection(string connectionString);
        dynamic Open(string adapterName, object settings);
    }

    internal class DatabaseOpener : IDatabaseOpener
    {
        private static readonly IAdapterFactory AdapterFactory = new CachingAdapterFactory();
        [ThreadStatic]
        private static Func<Database> _openDefault;
        [ThreadStatic]
        private static Func<string, Database> _openFile;
        [ThreadStatic]
        private static Func<string, Database> _openConnection;
        [ThreadStatic]
        private static Func<string, object, Database> _open;

        private static Func<Database> OpenDefaultImpl
        {
            get { return _openDefault ?? OpenDefaultMethod; }
        }

        private static Func<string,Database> OpenFileImpl
        {
            get { return _openFile ?? OpenFileMethod; }
        }

        private static Func<string, Database> OpenConnectionImpl
        {
            get { return _openConnection ?? OpenConnectionMethod; }
        }

        private static Func<string, object, Database> OpenImpl
        {
            get { return _open ?? OpenMethod; }
        }

        public dynamic OpenDefault()
        {
            return OpenDefaultImpl();
        }

        public dynamic OpenFile(string filename)
        {
            return OpenFileImpl(filename);
        }

        public dynamic OpenConnection(string connectionString)
        {
            return OpenConnectionImpl(connectionString);
        }

        public dynamic Open(string adapterName, object settings)
        {
            return OpenImpl(adapterName, settings);
        }

        public static void UseMockDatabase(Database database)
        {
            _openDefault = () => database;
            _openFile = _openConnection = (ignore) => database;
            _open = (ignore1, ignore2) => database;
        }

        public static void UseMockAdapter(Adapter adapter)
        {
            _openDefault = () => new Database(adapter);
            _openFile = _openConnection = (ignore) => new Database(adapter);
            _open = (ignore1, ignore2) => new Database(adapter);
        }

        public static void UseMockDatabase(Func<Database> databaseCreator)
        {
            _openDefault = () => databaseCreator();
            _openFile = _openConnection = (ignore) => databaseCreator();
            _open = (ignore1, ignore2) => databaseCreator();
        }

        public static void UseMockAdapter(Func<Adapter> adapterCreator)
        {
            _openDefault = () => new Database(adapterCreator());
            _openFile = _openConnection = (ignore) => new Database(adapterCreator());
            _open = (ignore1, ignore2) => new Database(adapterCreator());
        }

        private static Database OpenDefaultMethod()
        {
            return OpenConnectionMethod(DefaultConnectionString);
        }

        private static Database OpenFileMethod(string filename)
        {
            return new Database(AdapterFactory.Create("Ado", new { Filename = filename }));
        }

        private static Database OpenConnectionMethod(string connectionString)
        {
            return new Database(AdapterFactory.Create("Ado", new { ConnectionString = connectionString }));
        }

        private static Database OpenMethod(string adapterName, object settings)
        {
            return new Database(AdapterFactory.Create(adapterName, settings));
        }

        private static string DefaultConnectionString
        {
            get
            {
                return !string.IsNullOrWhiteSpace(Settings.Default.DefaultConnectionString)
                           ? Settings.Default.DefaultConnectionString
                           : Settings.Default.ConnectionString;
            }
        }
    }
}
