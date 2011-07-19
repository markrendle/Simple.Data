using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.SqlCe40Test
{
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
}
