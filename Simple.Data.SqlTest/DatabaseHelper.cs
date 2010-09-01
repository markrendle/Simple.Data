using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.SqlTest
{
    internal static class DatabaseHelper
    {
        public static dynamic Open()
        {
            return Database.OpenConnection(Properties.Settings.Default.ConnectionString);
        }
    }
}
