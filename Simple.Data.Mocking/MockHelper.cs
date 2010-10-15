using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.Mocking
{
    public static class MockHelper
    {
        public static void UseMockDatabase(Database mockDatabase)
        {
            DatabaseOpener.UseMockDatabase(mockDatabase);
        }

        public static void UseMockDatabase(Func<Database> mockDatabaseCreator)
        {
            DatabaseOpener.UseMockDatabase(mockDatabaseCreator);
        }

        public static void UseMockAdapter(IAdapter mockAdapter)
        {
            DatabaseOpener.UseMockAdapter(mockAdapter);
        }

        public static void UseMockAdapter(Func<IAdapter> mockAdapterCreator)
        {
            DatabaseOpener.UseMockAdapter(mockAdapterCreator());
        }
    }
}
