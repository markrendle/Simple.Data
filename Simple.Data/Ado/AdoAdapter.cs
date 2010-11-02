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
        private readonly IConnectionProvider _connectionProvider;
        private readonly DatabaseSchema _schema;

        public AdoAdapter(IConnectionProvider connectionProvider)
        {
            _connectionProvider = connectionProvider;
            _schema = DatabaseSchema.Get(_connectionProvider);
        }

        public IEnumerable<IDictionary<string, object>> Find(string tableName, SimpleExpression criteria)
        {
            if (criteria == null) return FindAll(tableName);

            var commandBuilder = new FindHelper(_schema).GetFindByCommand(tableName, criteria);
            return ExecuteQuery(commandBuilder);
        }

        public IDictionary<string, object> Insert(string tableName, IDictionary<string, object> data)
        {
            var table = _schema.FindTable(tableName);

            string columnList =
                data.Keys.Select(s => table.FindColumn(s).QuotedName).Aggregate((agg, next) => agg + "," + next);
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

        public int Update(string tableName, IDictionary<string, object> data, SimpleExpression criteria)
        {
            var commandBuilder = new UpdateHelper(_schema).GetUpdateCommand(tableName, data, criteria);
            return Execute(commandBuilder);
        }

        /// <summary>
        /// Deletes from the specified table.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="criteria">The expression to use as criteria for the delete operation.</param>
        /// <returns>The number of records which were deleted.</returns>
        public int Delete(string tableName, SimpleExpression criteria)
        {
            var commandBuilder = new DeleteHelper(_schema).GetDeleteCommand(tableName, criteria);
            return Execute(commandBuilder);
        }

        /// <summary>
        /// Gets the names of the fields which comprise the unique identifier for the specified table.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>A list of field names; an empty list if no key is defined.</returns>
        public IEnumerable<string> GetKeyFieldNames(string tableName)
        {
            return _schema.FindTable(tableName).PrimaryKey.AsEnumerable();
        }

        private IEnumerable<IDictionary<string, object>> FindAll(string tableName)
        {
            return ExecuteQuery("select * from " + _schema.FindTable(tableName).ActualName);
        }

        private IEnumerable<IDictionary<string, object>> ExecuteQuery(ICommandBuilder commandBuilder)
        {
            using (var connection = CreateConnection())
            {
                using (var command = commandBuilder.GetCommand(connection))
                {
                    return TryExecuteQuery(connection, command);
                }
            }
        }

        private IEnumerable<IDictionary<string, object>> ExecuteQuery(string sql, params object[] values)
        {
            using (var connection = CreateConnection())
            {
                using (var command = CommandHelper.Create(connection, sql, values))
                {
                    return TryExecuteQuery(connection, command);
                }
            }
        }

        private static IEnumerable<IDictionary<string, object>> TryExecuteQuery(DbConnection connection, IDbCommand command)
        {
            try
            {
                connection.Open();

                return command.ExecuteReader().ToDictionaries();
            }
            catch (DbException ex)
            {
                throw new AdoAdapterException(ex.Message, command);
            }
        }

        internal IDictionary<string, object> ExecuteSingletonQuery(string sql, params object[] values)
        {
            using (var connection = CreateConnection())
            {
                using (var command = CommandHelper.Create(connection, sql, values.ToArray()))
                {
                    try
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
                    catch (DbException ex)
                    {
                        throw new AdoAdapterException(ex.Message, command);
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
                    return TryExecute(connection, command);
                }
            }
        }

        private int Execute(ICommandBuilder commandBuilder)
        {
            using (var connection = CreateConnection())
            {
                using (var command = commandBuilder.GetCommand(connection))
                {
                    return TryExecute(connection, command);
                }
            }
        }

        private static int TryExecute(DbConnection connection, IDbCommand command)
        {
            try
            {
                connection.Open();
                return command.ExecuteNonQuery();
            }
            catch (DbException ex)
            {
                throw new AdoAdapterException(ex.Message, command);
            }
        }

        internal DbConnection CreateConnection()
        {
            return _connectionProvider.CreateConnection();
        }

        internal DatabaseSchema GetSchema()
        {
            return DatabaseSchema.Get(_connectionProvider);
        }
    }
}