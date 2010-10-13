using System.Data;
using System.Data.SqlClient;

namespace Simple.Data.Ado
{
    class SqlProvider : IConnectionProvider
    {
        private string _connectionString;

        public SqlProvider()
        {
            
        }

        public SqlProvider(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IDbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public void SetConnectionString(string connectionString)
        {
            _connectionString = connectionString;
        }
    }
}
