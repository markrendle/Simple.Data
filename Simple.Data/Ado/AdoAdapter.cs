using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using Simple.Data.Ado.Schema;

namespace Simple.Data.Ado
{
    internal class AdoAdapter : IAdapter, IAdapterWithRelation
    {
        private readonly IConnectionProvider _connectionProvider;
        private readonly DatabaseSchema _schema;
        private readonly Lazy<AdoAdapterRelatedFinder> _relatedFinder;

        public AdoAdapter(IConnectionProvider connectionProvider)
        {
            _connectionProvider = connectionProvider;
            _schema = DatabaseSchema.Get(_connectionProvider);
            _relatedFinder = new Lazy<AdoAdapterRelatedFinder>(CreateRelatedFinder);
        }

        private AdoAdapterRelatedFinder CreateRelatedFinder()
        {
            return new AdoAdapterRelatedFinder(this);
        }

        public IEnumerable<IDictionary<string, object>> Find(string tableName, SimpleExpression criteria)
        {
            return new AdoAdapterFinder(this).Find(tableName, criteria);
        }

        public IDictionary<string, object> Insert(string tableName, IDictionary<string, object> data)
        {
            return new AdoAdapterInserter(this).Insert(tableName, data);
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
            return _relatedFinder.Value.IsValidRelation(tableName, relatedTableName);
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
            return _relatedFinder.Value.FindRelated(tableName, row, relatedTableName);
        }
    }
}