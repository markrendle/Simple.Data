using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace Simple.Data.Ado
{
	class AdoAdapterMaxer
	{
		private readonly AdoAdapter _adapter;
        private readonly DbTransaction _transaction;
		private readonly DbConnection _connection;

        public AdoAdapterMaxer(AdoAdapter adapter) : this(adapter, null)
        {
        }

        public AdoAdapterMaxer(AdoAdapter adapter, DbTransaction transaction)
        {
			if (adapter == null) throw new ArgumentNullException("adapter");
			_adapter = adapter;

			if (transaction != null)
			{
				_transaction = transaction;
				_connection = transaction.Connection;
			}
		}		

		public object Max(string tableName, string fieldName)
		{
			var commandBuilder = new MaxHelper(_adapter.GetSchema()).GetMaxCommand(tableName, fieldName);
			return ExecuteQuery(commandBuilder);
		}

		private object ExecuteQuery(ICommandBuilder commandBuilder)
		{
			var connection = _connection ?? _adapter.CreateConnection();
			var command = commandBuilder.GetCommand(connection);
			command.Transaction = _transaction;
			return TryExecuteQuery(command);
		}

		private static object TryExecuteQuery(IDbCommand command)
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