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

        public AdoAdapterInserter(AdoAdapter adapter)
        {
            _adapter = adapter;
        }

        public IDictionary<string, object> Insert(string tableName, IDictionary<string, object> data)
        {
            var table = _adapter.GetSchema().FindTable(tableName);

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

        internal IDictionary<string, object> ExecuteSingletonQuery(string sql, params object[] values)
        {
            using (var connection = _adapter.CreateConnection())
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
            using (var connection = _adapter.CreateConnection())
            {
                using (var command = CommandHelper.Create(connection, sql, values.ToArray()))
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
    }
}
