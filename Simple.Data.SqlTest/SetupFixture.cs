using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace Simple.Data.SqlTest
{
    [SetUpFixture]
    public class SetupFixture
    {
        [SetUp]
        public void CreateStoredProcedures()
        {
            using (var cn = new SqlConnection(Properties.Settings.Default.ConnectionString))
            {
                cn.Open();
                using (var cmd = cn.CreateCommand())
                {
                    foreach (var sql in Regex.Split(Properties.Resources.DatabaseReset, @"^\s*GO\s*$", RegexOptions.Multiline))
                    {
                        cmd.CommandText = sql;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}
