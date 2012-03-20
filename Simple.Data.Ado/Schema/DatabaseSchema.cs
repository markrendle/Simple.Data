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

        private readonly ProviderHelper _providerHelper;
        private readonly ISchemaProvider _schemaProvider;
        private readonly Lazy<TableCollection> _lazyTables;
        private readonly Lazy<ProcedureCollection> _lazyProcedures;
        private string _defaultSchema;

        private DatabaseSchema(ISchemaProvider schemaProvider, ProviderHelper providerHelper)
        {
            _lazyTables = new Lazy<TableCollection>(CreateTableCollection);
            _lazyProcedures = new Lazy<ProcedureCollection>(CreateProcedureCollection);
            _schemaProvider = schemaProvider;
            _providerHelper = providerHelper;
        }

        public ProviderHelper ProviderHelper
        {
            get { return _providerHelper; }
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
            if (!string.IsNullOrWhiteSpace(DefaultSchema) && !(tableName.Contains(".")))
            {
                tableName = DefaultSchema + "." + tableName;
            }
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

        private string DefaultSchema
        {
            get { return _defaultSchema ?? (_defaultSchema = _schemaProvider.GetDefaultSchema() ?? string.Empty); }
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

        public static DatabaseSchema Get(IConnectionProvider connectionProvider, ProviderHelper providerHelper)
        {
            return Instances.GetOrAdd(connectionProvider.ConnectionString,
                                      sp => new DatabaseSchema(connectionProvider.GetSchemaProvider(), providerHelper));
        }

        public static void ClearCache()
        {
            Instances.Clear();
        }

        public ObjectName BuildObjectName(String text)
        {
            if (text == null) throw new ArgumentNullException("text");
            if (!text.Contains('.')) return new ObjectName(this.DefaultSchema, text);
            var schemaDotTable = text.Split('.');
            if (schemaDotTable.Length != 2) throw new InvalidOperationException("Could not parse table name.");
            return new ObjectName(schemaDotTable[0], schemaDotTable[1]);
        }

        public RelationType GetRelationType(string fromTableName, string toTableName)
        {
            var fromTable = FindTable(fromTableName);

            if (fromTable.GetMaster(toTableName) != null) return RelationType.ManyToOne;
            if (fromTable.GetDetail(toTableName) != null) return RelationType.OneToMany;
            return RelationType.None;
        }

        public bool IsProcedure(string procedureName)
        {
            return _lazyProcedures.Value.IsProcedure(procedureName);
        }
    }

    public enum RelationType
    {
        None,
        OneToMany,
        ManyToOne
    }
}