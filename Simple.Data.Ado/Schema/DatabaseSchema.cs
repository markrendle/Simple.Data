using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Simple.Data.Ado.Schema
{
    internal class DatabaseSchema
    {
        private static readonly ConcurrentDictionary<string, DatabaseSchema> Instances = new ConcurrentDictionary<string, DatabaseSchema>();

        private readonly ISchemaProvider _schemaProvider;
        private readonly Lazy<TableCollection> _lazyTables;

        private DatabaseSchema(ISchemaProvider schemaProvider)
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

        public Table FindTable(TableName tableName)
        {
            return _lazyTables.Value.Find(tableName);
        }

        private TableCollection CreateTableCollection()
        {
            return new TableCollection(_schemaProvider.GetTables()
                .Select(table => new Table(table.ActualName, table.Schema, table.Type, this)));
        }

        public string QuoteObjectName(string unquotedName)
        {
            return _schemaProvider.QuoteObjectName(unquotedName);
        }

        public string QuoteObjectName(TableName unquotedName)
        {
            return _schemaProvider.QuoteObjectName(unquotedName.Schema) + '.' + _schemaProvider.QuoteObjectName(unquotedName.Table);
        }

        public static DatabaseSchema Get(IConnectionProvider connectionProvider)
        {
            return Instances.GetOrAdd(connectionProvider.ConnectionString,
                                      sp => new DatabaseSchema(connectionProvider.GetSchemaProvider()));
        }
    }
}
