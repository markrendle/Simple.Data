using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data;
using System.Dynamic;
using Simple.Data.Ado;
using Simple.Data.Schema;

namespace Simple.Data
{
    public class Database : DynamicObject
    {
        private readonly IAdapter _adapter;
        private readonly IConnectionProvider _connectionProvider;
        private readonly IDbConnection _connection;
        private readonly string _connectionString;

        private Database(string connectionString)
        {
            _connectionString = connectionString;
        }

        internal Database(IAdapter adapter)
        {
            _adapter = adapter;
        }

        internal Database(IDbConnection connection)
        {
            _connection = connection;
        }

        internal Database(IConnectionProvider connectionProvider)
        {
            _adapter = new AdoAdapter(this, connectionProvider);
            _connectionProvider = connectionProvider;
        }

        public IAdapter Adapter
        {
            get { return _adapter; }
        }

        public static dynamic Open()
        {
            return new Database(new SqlProvider(Properties.Settings.Default.ConnectionString));
        }

        public static dynamic OpenConnection(string connectionString)
        {
            return new Database(new SqlProvider(connectionString));
        }

        public static dynamic OpenFile(string filename)
        {
            return new Database(ProviderHelper.GetProviderByFilename(filename));
        }

        internal void Execute(string sql, params object[] values)
        {
            using (var connection = CreateConnection())
            {
                using (var command = CommandHelper.Create(connection, sql, values.ToArray()))
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        public void Update(string table, IDictionary<string, object> data, IDictionary<string, object> criteria)
        {
            string set = string.Join(", ", data.Keys.Select(key => key + " = ?"));
            string where = string.Join(" and ", criteria.Keys.Select(key => key + " = ?"));

            string updateSql = "update " + table + " set " + set + " where " + where;

            Execute(updateSql, data.Values.Concat(criteria.Values).ToArray());
        }

        internal IDbConnection CreateConnection()
        {
            if (_connectionProvider != null) return _connectionProvider.CreateConnection();
            // Testability
            return _connection ?? new SqlConnection(_connectionString);
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return base.TryGetMember(binder, out result)
                   || NewDynamicTable(binder, out result);
        }

        private bool NewDynamicTable(GetMemberBinder binder, out object result)
        {
            result = new DynamicTable(binder.Name, this);
            return true;
        }

        public void Delete(string table, IDictionary<string, object> criteria)
        {
            string where = string.Join(" and ", criteria.Keys.Select(key => key + " = ?"));
            string deleteSql = "delete from " + table + " where " + where;

            Execute(deleteSql, criteria.Values.ToArray());
        }

        internal DatabaseSchema GetSchema()
        {
            return new DatabaseSchema(this);
        }
    }
}
