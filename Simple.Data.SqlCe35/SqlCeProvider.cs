using System;
using System.ComponentModel.Composition;
using System.Data.SqlServerCe;
using System.Data;
using Simple.Data.Ado;

namespace Simple.Data.SqlCe35
{
    [Export("sdf", typeof(IConnectionProvider))]
    public class SqlCe35Provider : IConnectionProvider
    {
        private string _connectionString;

        public SqlCe35Provider()
        {
            
        }

        public SqlCe35Provider(string filename)
        {
            _connectionString = string.Format("data source={0}", filename);
        }

        public SqlCe35Provider(string filename, string password)
        {
            _connectionString = string.Format("data source={0};password={1}", filename, password);
        }

        public void SetConnectionString(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IDbConnection CreateConnection()
        {
            return new SqlCeConnection(_connectionString);
        }
    }
}
