using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Shitty.Data.Ado.Schema;

namespace Shitty.Data.Ado.Test
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
            return new OleDbConnection();
        }

        public ISchemaProvider GetSchemaProvider()
        {
            return new StubSchemaProvider();
        }

        public string ConnectionString
        {
            get { return "stub"; }
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

    public class StubSchemaProvider : ISchemaProvider
    {
        public IEnumerable<Table> GetTables()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Column> GetColumns(Table table)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Procedure> GetStoredProcedures()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Parameter> GetParameters(Procedure storedProcedure)
        {
            throw new NotImplementedException();
        }

        public Key GetPrimaryKey(Table table)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ForeignKey> GetForeignKeys(Table table)
        {
            throw new NotImplementedException();
        }

        public string QuoteObjectName(string unquotedName)
        {
            throw new NotImplementedException();
        }

        public string NameParameter(string baseName)
        {
            throw new NotImplementedException();
        }

        public string GetDefaultSchema()
        {
            throw new NotImplementedException();
        }
    }

    [Export(typeof(ICustomInserter))]
    public class StubCustomInserter : ICustomInserter
    {
        public IDictionary<string, object> Insert(AdoAdapter adapter, string tableName, IDictionary<string, object> data, IDbTransaction transaction = null, bool resultRequired = false)
        {
            throw new NotImplementedException();
        }
    }
}
