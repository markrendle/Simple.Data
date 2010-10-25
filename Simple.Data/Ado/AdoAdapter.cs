using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Simple.Data.Schema;

namespace Simple.Data.Ado
{
    internal class AdoAdapter : IAdapter
    {
        private readonly Database _database;
        private readonly IConnectionProvider _connectionProvider;
        private readonly ISchemaProvider _schemaProvider;
        private readonly DatabaseSchema _schema;

        public AdoAdapter(Database database, IConnectionProvider connectionProvider)
        {
            _database = database;
            _connectionProvider = connectionProvider;
            _schemaProvider = _connectionProvider.GetSchemaProvider();
            _schema = new DatabaseSchema(_schemaProvider);
        }

        public IDictionary<string, object> Find(string tableName, SimpleExpression criteria)
        {
            var commandBuilder = new FindHelper(_schema).GetFindByCommand(tableName, criteria);
            return Query(commandBuilder).FirstOrDefault();
        }

        public IEnumerable<IDictionary<string, object>> FindAll(string tableName)
        {
            return Query("select * from " + _schema.FindTable(tableName).ActualName);
        }

        public IEnumerable<IDictionary<string, object>> FindAll(string tableName, SimpleExpression criteria)
        {
            var commandBuilder = new FindHelper(_schema).GetFindByCommand(tableName, criteria);
            return Query(commandBuilder);
        }

        public IEnumerable<IDictionary<string, object>> Query(ICommandBuilder commandBuilder)
        {
            using (var connection = CreateConnection())
            {
                using (var command = commandBuilder.GetCommand(connection))
                {
                    connection.Open();

                    return command.ExecuteReader().ToDictionaries();
                }
            }
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

        internal DbConnection CreateConnection()
        {
            return _connectionProvider.CreateConnection();
        }

        internal DataTable GetSchema(string collectionName)
        {
            return _schemaProvider.GetSchema(collectionName);
        }

        internal DataTable GetSchema(string collectionName, params string[] restrictionValues)
        {
            return _schemaProvider.GetSchema(collectionName, restrictionValues);
        }

        internal DatabaseSchema GetSchema()
        {
            return new DatabaseSchema(_connectionProvider.GetSchemaProvider());
        }
    }
}
