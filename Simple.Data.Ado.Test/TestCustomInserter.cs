using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Simple.Data.Ado.Schema;

namespace Simple.Data.Ado.Test
{
    [TestFixture]
    public class TestCustomInserter
    {
        [Test]
        public void ShouldResolveCustomInserter()
        {
            var helper = new ProviderHelper();
            var connectionProvider = new StubConnectionProvider();
            var actual = helper.GetCustomProvider<ICustomInserter>(connectionProvider);
            Assert.IsInstanceOf(typeof(StubCustomInserter), actual);
        }
    }

    public class StubConnectionProvider : IConnectionProvider
    {
        public void SetConnectionString(string connectionString)
        {
            throw new NotImplementedException();
        }

        public IDbConnection CreateConnection()
        {
            throw new NotImplementedException();
        }

        public ISchemaProvider GetSchemaProvider()
        {
            throw new NotImplementedException();
        }

        public string ConnectionString
        {
            get { throw new NotImplementedException(); }
        }

        public bool SupportsCompoundStatements
        {
            get { throw new NotImplementedException(); }
        }

        public string GetIdentityFunction()
        {
            throw new NotImplementedException();
        }

        public bool SupportsStoredProcedures
        {
            get { throw new NotImplementedException(); }
        }

        public IProcedureExecutor GetProcedureExecutor(AdoAdapter adapter, ObjectName procedureName)
        {
            throw new NotImplementedException();
        }
    }

    [Export(typeof(ICustomInserter))]
    public class StubCustomInserter : ICustomInserter
    {
        public IDictionary<string, object> Insert(AdoAdapter adapter, string tableName, IDictionary<string, object> data, IDbTransaction transaction = null)
        {
            throw new NotImplementedException();
        }
    }
}
