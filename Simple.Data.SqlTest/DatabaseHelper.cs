using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Simple.Data.SqlTest
{
    internal static class DatabaseHelper
    {
        public static dynamic Open()
        {
            return Database.Opener.OpenConnection(Properties.Settings.Default.ConnectionString);
        }

        public static void Reset()
        {
            using (var cn = new SqlConnection(Properties.Settings.Default.ConnectionString))
            {
                cn.Open();
                using (var cmd = cn.CreateCommand())
                {
                    foreach (var sql in Regex.Split(Properties.Resources.DatabaseReset, @"^\s*GO\s*$",RegexOptions.Multiline))
                    {
                        cmd.CommandText = sql;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}
