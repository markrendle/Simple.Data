using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Simple.Data.Ado;
using Simple.Data.Properties;

namespace Simple.Data
{
    internal static class DatabaseOpener
    {
        private static Func<Database> _openDefault = OpenDefaultMethod;
        private static Func<string, Database> _openFile = OpenFileMethod;
        private static Func<string, Database> _openConnection = OpenConnectionMethod;

        public static Database OpenDefault()
        {
            return _openDefault();
        }

        public static Database OpenFile(string filename)
        {
            return _openFile(filename);
        }

        public static Database OpenConnection(string connectionString)
        {
            return _openConnection(connectionString);
        }

        public static void UseMockDatabase(Database database)
        {
            _openDefault = () => database;
            _openFile = _openConnection = (ignore) => database;
        }

        public static void UseMockAdapter(IAdapter adapter)
        {
            _openDefault = () => new Database(adapter);
            _openFile = _openConnection = (ignore) => new Database(adapter);
        }

        public static void UseMockDatabase(Func<Database> databaseCreator)
        {
            _openDefault = () => databaseCreator();
            _openFile = _openConnection = (ignore) => databaseCreator();
        }

        public static void UseMockAdapter(Func<IAdapter> adapterCreator)
        {
            _openDefault = () => new Database(adapterCreator());
            _openFile = _openConnection = (ignore) => new Database(adapterCreator());
        }

        private static Database OpenDefaultMethod()
        {
            return new Database(new SqlProvider(Settings.Default.ConnectionString));
        }

        private static Database OpenFileMethod(string filename)
        {
            return new Database(ProviderHelper.GetProviderByFilename(filename));
        }

        private static Database OpenConnectionMethod(string connectionString)
        {
            return new Database(new SqlProvider(connectionString));
        }
    }
}
