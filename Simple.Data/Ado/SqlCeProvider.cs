using System.Data.SqlServerCe;
using System.Data;

namespace Simple.Data.Ado
{
    public class SqlCeProvider : IConnectionProvider
    {
        private readonly string _connectionString;

        public SqlCeProvider(string filename)
        {
            _connectionString = string.Format("data source={0}", filename);
        }

        public SqlCeProvider(string filename, string password)
        {
            _connectionString = string.Format("data source={0};password={1}", filename, password);
        }

        public IDbConnection CreateConnection()
        {
            return new SqlCeConnection(_connectionString);
        }
    }
}
