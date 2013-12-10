using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace Simple.Data.SqlTest
{
    using System.Diagnostics;

    [SetUpFixture]
    public class SetupFixture
    {
        [SetUp]
        public void CreateStoredProcedures()
        {
            AppDomain.CurrentDomain.SetData("DataDirectory", TestContext.CurrentContext.TestDirectory);
            var provider = new SqlServer.SqlConnectionProvider();
            Trace.Write("Loaded provider " + provider.GetType().Name);

            using (var cn = new SqlConnection(DatabaseHelper.ConnectionString))
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
