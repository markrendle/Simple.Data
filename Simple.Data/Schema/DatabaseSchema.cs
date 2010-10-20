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
        private readonly ISchemaProvider _schemaProvider;
        private readonly Lazy<TableCollection> _lazyTables;

        public DatabaseSchema(ISchemaProvider schemaProvider)
        {
            _lazyTables = new Lazy<TableCollection>(CreateTableCollection);
            _schemaProvider = schemaProvider;
        }

        public ISchemaProvider SchemaProvider
        {
            get { return _schemaProvider; }
        }

        public bool IsAvailable
        {
            get { return _schemaProvider != null; }
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
            var table = _schemaProvider.GetSchema("TABLES");

            var query = table.AsEnumerable().Select(
                row =>
                new Table(row["TABLE_NAME"].ToString(), row["TABLE_SCHEMA"].ToString(), row["TABLE_TYPE"].ToString(),
                          this));

            return new TableCollection(query);
        }
    }
}
