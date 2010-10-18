using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Data.Common;
using Simple.Data.Ado;

namespace Simple.Data.Schema
{
    internal class DatabaseSchema
    {
        private readonly Database _database;
        private readonly Lazy<TableCollection> _lazyTables;

        public DatabaseSchema(Database database)
        {
            _lazyTables = new Lazy<TableCollection>(CreateTableCollection);
            _database = database;
        }

        public Database Database
        {
            get { return _database; }
        }

        public bool IsAvailable
        {
            get { return _database != null; }
        }

        public IEnumerable<Table> Tables
        {
            get { return _lazyTables.Value.AsEnumerable(); }
        }

        public Table FindTable(string tableName)
        {
            return _lazyTables.Value.Find(tableName);
        }

        private TableCollection CreateTableCollection()
        {
            var table = ((AdoAdapter) _database.Adapter).GetSchema("Tables");

            var query = table.AsEnumerable().Select(
                row =>
                new Table(row["table_name"].ToString(), row["table_schema"].ToString(), row["table_type"].ToString(),
                          this));

            return new TableCollection(query);
        }
    }
}
