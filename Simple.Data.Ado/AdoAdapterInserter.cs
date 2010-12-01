using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Simple.Data.Ado
{
    class AdoAdapterInserter
    {
        private readonly AdoAdapter _adapter;
        private readonly DbTransaction _transaction;

        public AdoAdapterInserter(AdoAdapter adapter) : this(adapter, null)
        {
        }

        public AdoAdapterInserter(AdoAdapter adapter, DbTransaction transaction)
        {
            _adapter = adapter;
            _transaction = transaction;
        }

        public IDictionary<string, object> Insert(string tableName, IDictionary<string, object> data)
        {
            var table = _adapter.GetSchema().FindTable(tableName);

            string columnList =
                data.Keys.Select(s => table.FindColumn(s).QuotedName).Aggregate((agg, next) => agg + "," + next);
            string valueList = data.Keys.Select(s => "?").Aggregate((agg, next) => agg + "," + next);

            string insertSql = "insert into " + table.QualifiedName + " (" + columnList + ") values (" + valueList + ")";

            var identityColumn = table.Columns.FirstOrDefault(col => col.IsIdentity);

            if (identityColumn != null)
            {
                insertSql += "; select * from " + table.QualifiedName + " where " + identityColumn.QuotedName +
                             " = scope_identity()";
                return ExecuteSingletonQuery(insertSql, data.Values.ToArray());
            }

            Execute(insertSql, data.Values.ToArray());
            return null;
        }

        internal IDictionary<string, object> ExecuteSingletonQuery(string sql, params object[] values)
        {
            if (_transaction != null)
            {
                var command = CommandHelper.Create(_transaction.Connection, sql, values.ToArray());
                command.Transaction = _transaction;
                return TryExecuteSingletonQuery(command);
            }

            using (var connection = _adapter.CreateConnection())
            {
                using (var command = CommandHelper.Create(connection, sql, values.ToArray()))
                {
                    connection.Open();
                    return TryExecuteSingletonQuery(command);
                }
            }
        }

        private static IDictionary<string, object> TryExecuteSingletonQuery(IDbCommand command)
        {
            try
            {
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

            return null;
        }

        internal int Execute(string sql, params object[] values)
        {
            if (_transaction != null)
            {
                var command = CommandHelper.Create(_transaction.Connection, sql, values.ToArray());
                command.Transaction = _transaction;
                return TryExecute(command);
            }
            using (var connection = _adapter.CreateConnection())
            {
                using (var command = CommandHelper.Create(connection, sql, values.ToArray()))
                {
                    connection.Open();
                    return TryExecute(command);
                }
            }
        }

        private static int TryExecute(IDbCommand command)
        {
            try
            {
                return command.ExecuteNonQuery();
            }
            catch (DbException ex)
            {
                throw new AdoAdapterException(ex.Message, command);
            }
        }
    }
}
