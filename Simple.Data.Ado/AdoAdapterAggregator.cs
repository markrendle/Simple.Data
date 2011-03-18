using System;
using System.Data;
using System.Data.Common;

namespace Simple.Data.Ado
{
	class AdoAdapterAggregator
	{
		private readonly AdoAdapter _adapter;
        private readonly DbTransaction _transaction;
		private readonly DbConnection _connection;

        public AdoAdapterAggregator(AdoAdapter adapter) : this(adapter, null)
        {
        }

        public AdoAdapterAggregator(AdoAdapter adapter, DbTransaction transaction)
        {
			if (adapter == null) throw new ArgumentNullException("adapter");
			_adapter = adapter;

			if (transaction != null)
			{
				_transaction = transaction;
				_connection = transaction.Connection;
			}
		}

		public object Max(string tableName, string columnName, SimpleExpression criteria)
		{
			var commandBuilder = new AggregationHelper(_adapter.GetSchema()).GetMaxCommand(tableName, columnName, criteria);
			return ExecuteScalar(commandBuilder);
		}

		public object Min(string tableName, string columnName, SimpleExpression criteria)
		{
			var commandBuilder = new AggregationHelper(_adapter.GetSchema()).GetMinCommand(tableName, columnName, criteria);
			return ExecuteScalar(commandBuilder);
		}


		private object ExecuteScalar(ICommandBuilder commandBuilder)
		{
			var connection = _connection ?? _adapter.CreateConnection();
			var command = commandBuilder.GetCommand(connection);
			command.Transaction = _transaction;
			return TryExecuteScalar(command);
		}

		private static object TryExecuteScalar(IDbCommand command)
		{
			try
			{
				command.Connection.Open();
				return command.ExecuteScalar();
			}
			catch (DbException ex)
			{
				throw new AdoAdapterException(ex.Message, command);
			}
		}
	}
}