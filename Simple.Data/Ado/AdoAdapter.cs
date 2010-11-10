using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using Simple.Data.Ado.Schema;

namespace Simple.Data.Ado
{
    internal class AdoAdapter : IAdapter, IAdapterWithRelation
    {
        private readonly IConnectionProvider _connectionProvider;
        private readonly DatabaseSchema _schema;

        private readonly Lazy<ConcurrentDictionary<Tuple<string, string>, TableJoin>> _tableJoins =
            new Lazy<ConcurrentDictionary<Tuple<string, string>, TableJoin>>(
                () => new ConcurrentDictionary<Tuple<string, string>, TableJoin>(), LazyThreadSafetyMode.ExecutionAndPublication
                );

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

        /// <summary>
        /// Determines whether a relation is valid.
        /// </summary>
        /// <param name="tableName">Name of the known table.</param>
        /// <param name="relatedTableName">Name of the table to test.</param>
        /// <returns>
        /// 	<c>true</c> if there is a valid relation; otherwise, <c>false</c>.
        /// </returns>
        public bool IsValidRelation(string tableName, string relatedTableName)
        {
            return TryJoin(tableName, relatedTableName) != null;
        }

        /// <summary>
        /// Finds data from a "table" related to the specified "table".
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="row"></param>
        /// <param name="relatedTableName"></param>
        /// <returns>The list of records matching the criteria. If no records are found, return an empty list.</returns>
        /// <remarks>When implementing the <see cref="IAdapter"/> interface, if relationships are not possible, throw a <see cref="NotSupportedException"/>.</remarks>
        public IEnumerable<IDictionary<string, object>> FindRelated(string tableName, IDictionary<string, object> row, string relatedTableName)
        {
            var join = TryJoin(tableName, relatedTableName);
            if (join == null) throw new AdoAdapterException("Could not resolve relationship.");

            return join.Master == _schema.FindTable(tableName) ? GetDetail(row, join) : GetMaster(row, join);
        }

        private TableJoin TryJoin(string tableName, string relatedTableName)
        {
            return _tableJoins.Value.GetOrAdd(Tuple.Create(tableName, relatedTableName),
                                              t => TryCreateJoin(t.Item1, t.Item2));
        }

        private TableJoin TryCreateJoin(string tableName, string relatedTableName)
        {
            return TryMasterJoin(tableName, relatedTableName) ?? TryDetailJoin(tableName, relatedTableName);
        }

        private TableJoin TryMasterJoin(string tableName, string relatedTableName)
        {
            return _schema.FindTable(tableName).GetMaster(relatedTableName);
        }

        private TableJoin TryDetailJoin(string tableName, string relatedTableName)
        {
            return _schema.FindTable(tableName).GetDetail(relatedTableName);
        }

        private IEnumerable<IDictionary<string,object>> GetMaster(IDictionary<string,object> row, TableJoin masterJoin)
        {
            var criteria = new Dictionary<string, object> { { masterJoin.MasterColumn.ActualName, row[masterJoin.DetailColumn.HomogenizedName] } };
            yield return Find(masterJoin.Master.ActualName,
                                       ExpressionHelper.CriteriaDictionaryToExpression(masterJoin.Master.ActualName,
                                                                                       criteria)).FirstOrDefault();
        }

        private IEnumerable<IDictionary<string, object>> GetDetail(IDictionary<string, object> row, TableJoin join)
        {
            var criteria = new Dictionary<string, object> { { join.DetailColumn.ActualName, row[join.MasterColumn.HomogenizedName] } };
            return Find(join.Detail.ActualName,
                                          ExpressionHelper.CriteriaDictionaryToExpression(join.Detail.ActualName,
                                                                                          criteria));
        }
    }
}