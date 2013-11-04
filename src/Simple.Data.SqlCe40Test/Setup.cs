namespace Simple.Data.SqlCe40Test
{
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using NUnit.Framework;

    [SetUpFixture]
    public class Setup
    {
        [SetUp]
        public void ForceLoadOfSimpleDataSqlCe40()
        {
            var provider = new SqlCe40.SqlCe40ConnectionProvider();
            Trace.Write("Loaded provider.");

            var executionDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase.Substring(8));
            Debug.Assert(!ReferenceEquals(executionDirectory, null));

            var sourcePath = Path.Combine(executionDirectory, "TestDatabase.sdf");
            var databasePath = Path.Combine(executionDirectory, "TestDatabaseCopy.sdf");

            File.Copy(sourcePath, databasePath, true);
        }
    }
}