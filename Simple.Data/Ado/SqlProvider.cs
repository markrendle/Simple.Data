using System.Data;
using System.Data.SqlClient;

namespace Simple.Data.Ado
{
    class SqlProvider : IConnectionProvider
    {
        private readonly string _connectionString;

        public SqlProvider(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IDbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
