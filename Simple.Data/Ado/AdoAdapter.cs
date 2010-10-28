using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Simple.Data.Ado.Schema;

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
            var table = _schema.FindTable(tableName);
            string columnList = data.Keys.Select(s => table.FindColumn(s).QuotedName).Aggregate((agg, next) => agg + "," + next);
            string valueList = data.Keys.Select(s => "?").Aggregate((agg, next) => agg + "," + next);

            string insertSql = "insert into " + table.QuotedName + " (" + columnList + ") values (" + valueList + ")";

            var identityColumn = table.Columns.FirstOrDefault(col => col.IsIdentity);

            if (identityColumn != null)
            {
                insertSql += "; select * from " + table.QuotedName + " where " + identityColumn.QuotedName +
                             " = scope_identity()";
                return ExecuteSingletonQuery(insertSql, data.Values.ToArray());
            }

            Execute(insertSql, data.Values.ToArray());
            return null;
        }

        internal IDictionary<string, object> ExecuteSingletonQuery(string sql, params object[] values)
        {
            using (var connection = CreateConnection())
            {
                using (var command = CommandHelper.Create(connection, sql, values.ToArray()))
                {
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return reader.ToDictionary();
                        }
                    }
                }
            }

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

        internal DatabaseSchema GetSchema()
        {
            return new DatabaseSchema(_connectionProvider.GetSchemaProvider());
        }
    }
}
