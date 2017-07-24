using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shitty.Data.SqlCe40Test
{
    using System.Diagnostics;
    using Ado;
    using NUnit.Framework;
    using SqlCe40;

    [TestFixture]
    class ConnectionProviderTest
    {
        [Test]
        public void SqlCeDoesNotSupportStoredProcedures()
        {
            IConnectionProvider target = new SqlCe40ConnectionProvider();
            Assert.IsFalse(target.SupportsStoredProcedures);
            Assert.Throws<NotSupportedException>(() => target.GetProcedureExecutor(null, null));
        }

        [Test]
        public void SqlCeDoesNotSupportCompoundStatements()
        {
            IConnectionProvider target = new SqlCe40ConnectionProvider();
            Assert.IsFalse(target.SupportsCompoundStatements);
        }
    }

    [SetUpFixture]
    public class Setup
    {
        [SetUp]
        public void ForceLoadOfSimpleDataSqlCe40()
        {
            var provider = new SqlCe40.SqlCe40ConnectionProvider();
            Trace.Write("Loaded provider.");
        }
    }
}
