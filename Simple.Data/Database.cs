using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data;
using System.Dynamic;

namespace Simple.Data
{
    public class Database : DynamicObject
    {
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

        public static Database Open()
        {
            return new Database(Properties.Settings.Default.ConnectionString);
        }

        public static Database OpenConnection(string connectionString)
        {
            return new Database(connectionString);
        }

        public IEnumerable<dynamic> Query(string sql, params object[] values)
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

        public void Execute(string sql, params object[] values)
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

        internal IDbConnection CreateConnection()
        {
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
    }
}
