using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using Simple.Data.Ado;

namespace Simple.Data.SqlTest
{
    class ObservableDataReaderTest
    {
        private static SqlConnection MakeConnection()
        {
            return new SqlConnection(@"data source=.\SQLSERVER2008;initial catalog=SimpleTest;integrated security=true");
        }

        private static SqlCommand MakeCommand()
        {
            return new SqlCommand("select * from Users", MakeConnection());
        }

        private static List<IDictionary<string, object>> _canonicalResults;
        private static readonly object Sync = new object();

        private static List<IDictionary<string, object>> CanonicalResults
        {
            get
            {
                CreateCanonicalResultsIfNull();

                return _canonicalResults;
            }
        }

        private static void CreateCanonicalResultsIfNull()
        {
            if (_canonicalResults == null)
            {
                lock (Sync)
                {
                    if (_canonicalResults == null)
                    {
                        CreateCanonicalResults();
                    }
                }
            }
        }

        private static void CreateCanonicalResults()
        {
            _canonicalResults = new List<IDictionary<string, object>>();
            var cmd = MakeCommand();
            using (cmd.Connection)
            using (cmd)
            {
                cmd.Connection.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        _canonicalResults.Add(reader.ToDictionary());
                    }
                }
            }
        }

        // ReSharper disable InconsistentNaming

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ToObservable_With_Open_Connection_Should_Throw_Exception()
        {
            var cn = MakeConnection();
            var cmd = cn.CreateCommand();
            cn.Open();
            cmd.ToObservable();
        }

        [Test]
        public void ToAsyncEnumerable_Should_Work_After_Pause()
        {
            var users = MakeCommand().ToAsyncEnumerable();
            Thread.Sleep(100);
            foreach (var user in users)
            {
                Console.WriteLine(user["Id"]);
            }

        }

        [Test]
        public void EnumeratorShouldRunOnOriginalThread()
        {
            var threadId = Thread.CurrentThread.ManagedThreadId;
            foreach (var x in MakeCommand().ToAsyncEnumerable())
            {
                Assert.AreEqual(threadId, Thread.CurrentThread.ManagedThreadId);
            }
        }
        // ReSharper restore InconsistentNaming
    }
}
