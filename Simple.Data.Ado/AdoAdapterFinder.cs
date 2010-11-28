using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Simple.Data.Ado
{
    class AdoAdapterFinder
    {
        private readonly AdoAdapter _adapter;

        public AdoAdapterFinder(AdoAdapter adapter)
        {
            _adapter = adapter;
        }

        public IEnumerable<IDictionary<string, object>> Find(string tableName, SimpleExpression criteria)
        {
            if (criteria == null) return FindAll(TableName.Parse(tableName));

            var commandBuilder = new FindHelper(_adapter.GetSchema()).GetFindByCommand(TableName.Parse(tableName), criteria);
            return ExecuteQuery(commandBuilder);
        }

        private IEnumerable<IDictionary<string, object>> FindAll(TableName tableName)
        {
            return ExecuteQuery("select * from " + _adapter.GetSchema().FindTable(tableName).ActualName);
        }

        private IEnumerable<IDictionary<string, object>> ExecuteQuery(ICommandBuilder commandBuilder)
        {
            var connection = _adapter.CreateConnection();
            var command = commandBuilder.GetCommand(connection);
            return TryExecuteQuery(connection, command);
        }

        private IEnumerable<IDictionary<string, object>> ExecuteQuery(string sql, params object[] values)
        {
            var connection = _adapter.CreateConnection();
            var command = CommandHelper.Create(connection, sql, values);
            return command.ToAsyncEnumerable();
        }

        private static IEnumerable<IDictionary<string, object>> TryExecuteQuery(DbConnection connection, IDbCommand command)
        {
            try
            {
                return command.ToAsyncEnumerable();
            }
            catch (DbException ex)
            {
                throw new AdoAdapterException(ex.Message, command);
            }
        }
    }
}
