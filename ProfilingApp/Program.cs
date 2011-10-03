using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProfilingApp
{
    using System.Data.SqlClient;
    using System.Text.RegularExpressions;

    class Program
    {
        static void Main(string[] args)
        {
            ResetDatabase();

            new QueryWithCountTask().Run();
        }

        private static void ResetDatabase()
        {
            using (var cn = new SqlConnection(Properties.Settings.Default.ConnectionString))
            {
                cn.Open();
                using (var cmd = cn.CreateCommand())
                {
                    foreach (
                        var sql in Regex.Split(Properties.Resources.DatabaseResetSql, @"^\s*GO\s*$", RegexOptions.Multiline))
                    {
                        cmd.CommandText = sql;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}
