using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Simple.Data.Ado
{
    internal class AdoAdapter : IAdapter
    {
        private readonly Database _database;
        private readonly IConnectionProvider _connectionProvider;

        public AdoAdapter(Database database, IConnectionProvider connectionProvider)
        {
            _database = database;
            _connectionProvider = connectionProvider;
        }

        public IDictionary<string, object> Find(string tableName, IDictionary<string, object> criteria)
        {
            return FindAll(tableName, criteria).FirstOrDefault();
        }

        public IDictionary<string, object> Find(string tableName, SimpleExpression criteria)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IDictionary<string, object>> FindAll(string tableName)
        {
            return Query("select * from " + tableName);
        }

        public IEnumerable<IDictionary<string, object>> FindAll(string tableName, IDictionary<string, object> criteria)
        {
            var sql = FindHelper.GetFindBySql(tableName, criteria);
            return Query(sql, criteria.Values.ToArray());
        }

        public IEnumerable<IDictionary<string, object>> Query(string sql, params object[] values)
        {
            using (var connection = CreateConnection())
            {
                using (var command = CommandHelper.Create(connection, sql, values))
                {
                    connection.Open();

                    return command.ExecuteReader().ToDictionaries();
                }
            }
        }
        
        public IDictionary<string, object> Insert(string tableName, IDictionary<string, object> data)
        {
            string columnList = data.Keys.Aggregate((agg, next) => agg + "," + next);
            string valueList = data.Keys.Select(s => "?").Aggregate((agg, next) => agg + "," + next);

            string insertSql = "insert into " + tableName + " (" + columnList + ") values (" + valueList + ")";

            Execute(insertSql, data.Values.ToArray());

            return null;
        }

        internal int Execute(string sql, params object[] values)
        {
            using (var connection = CreateConnection())
            {
                using (var command = CommandHelper.Create(connection, sql, values.ToArray()))
                {
                    connection.Open();
                    return command.ExecuteNonQuery();
                }
            }
        }

        public int Update(string tableName, IDictionary<string, object> data, IDictionary<string, object> criteria)
        {
            string set = string.Join(", ", data.Keys.Select(key => key + " = ?"));
            string where = string.Join(" and ", criteria.Keys.Select(key => key + " = ?"));

            string updateSql = "update " + tableName + " set " + set + " where " + where;

            return Execute(updateSql, data.Values.Concat(criteria.Values).ToArray());
        }

        public int Delete(string tableName, IDictionary<string, object> criteria)
        {
            string where = string.Join(" and ", criteria.Keys.Select(key => key + " = ?"));
            string deleteSql = "delete from " + tableName + " where " + where;

            return Execute(deleteSql, criteria.Values.ToArray());
        }

        internal IDbConnection CreateConnection()
        {
            return _connectionProvider.CreateConnection();
        }
    }
}
