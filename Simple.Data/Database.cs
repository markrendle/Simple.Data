using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data;
using System.Dynamic;
using Simple.Data.Schema;
using Simple.Data.SqlCe;

namespace Simple.Data
{
    public class Database : DynamicObject
    {
        private readonly IConnectionProvider _connectionProvider;
        private readonly IDbConnection _connection;
        private readonly string _connectionString;
        private readonly CommandHelper _commandHelper = new CommandHelper();

        private Database(string connectionString)
        {
            _connectionString = connectionString;
        }

        internal Database(IDbConnection connection)
        {
            _connection = connection;
        }

        internal Database(IConnectionProvider connectionProvider)
        {
            _connectionProvider = connectionProvider;
        }

        public static dynamic Open()
        {
            return new Database(new SqlProvider(Properties.Settings.Default.ConnectionString));
        }

        public static dynamic OpenConnection(string connectionString)
        {
            return new Database(connectionString);
        }

        public static dynamic OpenFile(string filename)
        {
            return new Database(ProviderHelper.GetProviderByFilename(filename));
        }

        internal IEnumerable<dynamic> Query(string sql, params object[] values)
        {
            using (var connection = CreateConnection())
            {
                using (var command = CommandHelper.Create(connection, sql, values))
                {
                    connection.Open();

                    return command.ExecuteReader().ToDynamicList();
                }
            }
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

        public void Insert(string table, IDictionary<string, object> data)
        {
            string columnList = data.Keys.Aggregate((agg, next) => agg + "," + next);
            string valueList = data.Keys.Select(s => "?").Aggregate((agg, next) => agg + "," + next);

            string insertSql = "insert into " + table + " (" + columnList + ") values (" + valueList + ")";

            Execute(insertSql, data.Values.ToArray());
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

        internal CommandHelper CommandHelper
        {
            get { return _commandHelper; }
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
