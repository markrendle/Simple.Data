using System.Data;
using System.Data.Common;
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

        public DbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public void SetConnectionString(string connectionString)
        {
            _connectionString = connectionString;
        }

        public DataTable GetSchema(string collectionName)
        {
            using (var cn = CreateConnection())
            {
                cn.Open();
                return cn.GetSchema(collectionName);
            }
        }

        public DataTable GetSchema(string collectionName, params string[] restrictionValues)
        {
            using (var cn = CreateConnection())
            {
                cn.Open();
                return cn.GetSchema(collectionName, restrictionValues);
            }
        }
    }
}
