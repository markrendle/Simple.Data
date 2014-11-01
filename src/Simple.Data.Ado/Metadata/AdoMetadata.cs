namespace Simple.Data.Ado.Metadata
{
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;
    using Schema;

    public class AdoMetadata : IMetadata
    {
        private static readonly IList<Column> EmptyColumns = new Column[0];
        private readonly DatabaseSchema _schema;

        public AdoMetadata(AdoAdapter adapter)
        {
            _schema = adapter.GetSchema();
        }

        public Task<IList<Table>> GetTables()
        {
            IList<Table> tables = _schema.Tables.Select(t => new Table(t.Schema, t.ActualName)).ToList();
            return Task.FromResult(tables);
        }

        public Task<IList<Column>> GetColumns(Table table)
        {
            IList<Column> columns;
            var schemaTable = _schema.FindTable(new ObjectName(table.Schema, table.Name));
            if (schemaTable == null)
            {
                columns = EmptyColumns;
            }
            else
            {
                columns = schemaTable.Columns.Select(c => new Column(c.ActualName, c.DbType, c.TypeName, c.IsIdentity, c.MaxLength)).ToList();
            }
            return Task.FromResult(columns);
        }
    }

    public class Table
    {
        private readonly string _schema;
        private readonly string _name;

        public Table(string schema, string name)
        {
            _schema = schema;
            _name = name;
        }

        public string Schema
        {
            get { return _schema; }
        }

        public string Name
        {
            get { return _name; }
        }
    }

    public class Column
    {
        private readonly string _name;
        private readonly DbType _dbType;
        private readonly string _typeName;
        private readonly bool _isIdentity;
        private readonly int _maxLength;

        public Column(string name, DbType dbType, string typeName, bool isIdentity, int maxLength)
        {
            _name = name;
            _dbType = dbType;
            _typeName = typeName;
            _isIdentity = isIdentity;
            _maxLength = maxLength;
        }

        public int MaxLength
        {
            get { return _maxLength; }
        }

        public bool IsIdentity
        {
            get { return _isIdentity; }
        }

        public string TypeName
        {
            get { return _typeName; }
        }

        public DbType DbType
        {
            get { return _dbType; }
        }

        public string Name
        {
            get { return _name; }
        }
    }
}