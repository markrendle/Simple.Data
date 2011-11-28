using System;
using System.Data;
using System.Data.Common;
using Simple.Data.Ado;
using Simple.Data.Ado.Schema;

namespace Simple.Data.Mocking.Ado
{
    public class MockConnectionProvider : IConnectionProvider
    {
        private readonly DbConnection _connection;
        private readonly MockSchemaProvider _mockSchemaProvider;
        private string _identityFunction;

        public MockConnectionProvider(DbConnection connection, MockSchemaProvider mockSchemaProvider, string identityFunction = null)
        {
            _connection = connection;
            _mockSchemaProvider = mockSchemaProvider;
            _identityFunction = identityFunction;
        }

        public void SetConnectionString(string connectionString)
        {
            throw new NotImplementedException();
        }

        public IDbConnection CreateConnection()
        {
            return _connection;
        }

        public ISchemaProvider GetSchemaProvider()
        {
            return _mockSchemaProvider;
        }

        public string ConnectionString
        {
            get { return _connection.ConnectionString; }
        }

        public string GetIdentityFunction()
        {
            return _identityFunction;
        }

        public void SetIdentityFunction(string identityFunction)
        {
            _identityFunction = identityFunction;
        }

        public bool TryGetNewRowSelect(Table table, out string selectSql)
        {
            selectSql = null;
            return false;
        }

        public bool SupportsStoredProcedures
        {
            get { return true; }
        }

        public IProcedureExecutor GetProcedureExecutor(AdoAdapter adapter, ObjectName procedureName)
        {
            return new ProcedureExecutor(adapter, procedureName);
        }

        private bool _supportsCompoundStatements = true;
        public bool SupportsCompoundStatements
        {
            get { return _supportsCompoundStatements; }
            set { _supportsCompoundStatements = value; }
        }
    }
}
