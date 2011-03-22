using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using Simple.Data.Extensions;

namespace Simple.Data.Ado
{
    class AdoAdapterInserter
    {
        private readonly AdoAdapter _adapter;
        private readonly IDbTransaction _transaction;

        public AdoAdapterInserter(AdoAdapter adapter) : this(adapter, null)
        {
        }

        public AdoAdapterInserter(AdoAdapter adapter, IDbTransaction transaction)
        {
            _adapter = adapter;
            _transaction = transaction;
        }

        public IDictionary<string, object> Insert(string tableName, IDictionary<string, object> data)
        {
            var table = _adapter.GetSchema().FindTable(tableName);

            data = data.Where(kvp => !table.FindColumn(kvp.Key).IsIdentity)
                .ToDictionary();
            string columnList =
                data.Keys.Select(table.FindColumn)
                //.Where(c => !c.IsIdentity)
                .Select(c => c.QuotedName)
                .Aggregate((agg, next) => agg + "," + next);
            string valueList = columnList.Split(',').Select(s => "?").Aggregate((agg, next) => agg + "," + next);

            string insertSql = "insert into " + table.QualifiedName + " (" + columnList + ") values (" + valueList + ")";

            var identityFunction = _adapter.GetIdentityFunction();
            if (!string.IsNullOrWhiteSpace(identityFunction))
            {
                var identityColumn = table.Columns.FirstOrDefault(col => col.IsIdentity);

                if (identityColumn != null)
                {
                    var selectSql = "select * from " + table.QualifiedName + " where " + identityColumn.QuotedName +
                                     " = " + identityFunction;
                    if (_adapter.ProviderSupportsCompoundStatements)
                    {
                        insertSql += "; " + selectSql;
                        return ExecuteSingletonQuery(insertSql, data.Values.ToArray());
                    }
                    else
                    {
                        return ExecuteSingletonQuery(insertSql, selectSql, data.Values.ToArray());
                    }
                }
            }

            Execute(insertSql, data.Values.ToArray());
            return null;
        }

        internal IDictionary<string, object> ExecuteSingletonQuery(string sql, params object[] values)
        {
            if (_transaction != null)
            {
                var command = new CommandHelper(_adapter.SchemaProvider).Create(_transaction.Connection, sql, values.ToArray());
                command.Transaction = _transaction;
                return TryExecuteSingletonQuery(command);
            }

            using (var connection = _adapter.CreateConnection())
            {
                using (var command = new CommandHelper(_adapter.SchemaProvider).Create(connection, sql, values.ToArray()))
                {
                    connection.Open();
                    return TryExecuteSingletonQuery(command);
                }
            }
        }

        internal IDictionary<string, object> ExecuteSingletonQuery(string insertSql, string selectSql, params object[] values)
        {
            if (_transaction != null)
            {
                var command = new CommandHelper(_adapter.SchemaProvider).Create(_transaction.Connection, insertSql, values.ToArray());
                command.Transaction = _transaction;
                TryExecute(command);
                command.CommandText = selectSql;
                command.Parameters.Clear();
                return TryExecuteSingletonQuery(command);
            }

            using (var connection = _adapter.CreateConnection())
            {
                using (var command = new CommandHelper(_adapter.SchemaProvider).Create(connection, insertSql, values.ToArray()))
                {
                    connection.Open();
                    TryExecute(command);
                    command.CommandText = selectSql;
                    command.Parameters.Clear();
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
                var command = new CommandHelper(_adapter.SchemaProvider).Create(_transaction.Connection, sql, values.ToArray());
                command.Transaction = _transaction;
                return TryExecute(command);
            }
            using (var connection = _adapter.CreateConnection())
            {
                using (var command = new CommandHelper(_adapter.SchemaProvider).Create(connection, sql, values.ToArray()))
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
