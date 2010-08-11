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
            return new TableCollection(
                _database.Query("select TABLE_NAME, TABLE_SCHEMA from INFORMATION_SCHEMA.TABLES")
                .Select(d => new Table(d.TableName.ToString(), d.TableSchema.ToString(), this))
                );
        }
    }
}
