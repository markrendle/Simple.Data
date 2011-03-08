using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Simple.Data.Ado.Schema
{
    public class DatabaseSchema
    {
        private static readonly ConcurrentDictionary<string, DatabaseSchema> Instances = new ConcurrentDictionary<string, DatabaseSchema>();

        private readonly ISchemaProvider _schemaProvider;
        private readonly Lazy<TableCollection> _lazyTables;
        private readonly Lazy<ProcedureCollection> _lazyProcedures;

        private DatabaseSchema(ISchemaProvider schemaProvider)
        {
            _lazyTables = new Lazy<TableCollection>(CreateTableCollection);
            _lazyProcedures = new Lazy<ProcedureCollection>(CreateProcedureCollection);
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

        public Table FindTable(ObjectName tableName)
        {
            return _lazyTables.Value.Find(tableName);
        }

        public Procedure FindProcedure(string procedureName)
        {
            return _lazyProcedures.Value.Find(procedureName);
        }

        public Procedure FindProcedure(ObjectName procedureName)
        {
            return _lazyProcedures.Value.Find(procedureName);
        }

        private TableCollection CreateTableCollection()
        {
            return new TableCollection(_schemaProvider.GetTables()
                .Select(table => new Table(table.ActualName, table.Schema, table.Type, this)));
        }

        private ProcedureCollection CreateProcedureCollection()
        {
            return new ProcedureCollection(_schemaProvider.GetStoredProcedures()
                                                     .Select(
                                                         proc =>
                                                         new Procedure(proc.Name, proc.SpecificName, proc.Schema,
                                                                             this)));
        }

        public string QuoteObjectName(string unquotedName)
        {
            return _schemaProvider.QuoteObjectName(unquotedName);
        }

        public string QuoteObjectName(ObjectName unquotedName)
        {
            if (!string.IsNullOrWhiteSpace(unquotedName.Schema))
                return _schemaProvider.QuoteObjectName(unquotedName.Schema) + '.' + _schemaProvider.QuoteObjectName(unquotedName.Name);
            else
                return _schemaProvider.QuoteObjectName(unquotedName.Name);
        }

        public static DatabaseSchema Get(IConnectionProvider connectionProvider)
        {
            return Instances.GetOrAdd(connectionProvider.ConnectionString,
                                      sp => new DatabaseSchema(connectionProvider.GetSchemaProvider()));
        }
    }
}
