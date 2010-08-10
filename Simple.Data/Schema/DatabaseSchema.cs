using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Data.Common;

namespace Simple.Data.Schema
{
    internal class DatabaseSchema
    {
        private readonly DbConnection _connection;
        private readonly List<Table> _tables;

        public DatabaseSchema(Func<IDbConnection> connectionFunc)
        {
            _connection = connectionFunc() as DbConnection;

            if (_connection != null)
            {
                var tables = _connection.GetSchema("Tables").AsEnumerable()
                    .Select(row => new Table(row.Field<string>("table_name"), row.Field<string>("table_schema")));
                _tables = new List<Table>(tables);
            }
        }

        public bool IsAvailable
        {
            get { return _connection != null; }
        }

        public Table GetTable(string tableName)
        {
            return GetTableWithExactName(tableName)
                   ?? GetTableWithPluralName(tableName)
                   ?? GetTableWithSingularName(tableName);
        }

        private Table GetTableWithExactName(string tableName)
        {
            return _tables
                .Where(t => t.Name.Equals(tableName, StringComparison.InvariantCultureIgnoreCase))
                .FirstOrDefault();
        }

        private Table GetTableWithPluralName(string tableName)
        {
            return GetTableWithExactName(tableName.Pluralize());
        }

        private Table GetTableWithSingularName(string tableName)
        {
            if (tableName.IsPlural())
            {
                return GetTableWithExactName(tableName.Singularize());
            }

            return null;
        }
    }
}
