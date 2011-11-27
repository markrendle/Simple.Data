using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Simple.Data.SqlTest
{
    internal static class DatabaseHelper
    {
		public static readonly string ConnectionString =
			"Data Source=10.37.129.4;Initial Catalog=SimpleTest;User ID=SimpleUser;Password=SimplePassword";
		
        public static dynamic Open()
        {
            return Database.Opener.OpenConnection(ConnectionString);
        }

        public static void Reset()
        {
            using (var cn = new SqlConnection(ConnectionString))
            {
                cn.Open();
                using (var cmd = cn.CreateCommand())
                {
                    cmd.CommandText = "TestReset";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
