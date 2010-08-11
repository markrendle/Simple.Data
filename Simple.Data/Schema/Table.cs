using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.Schema
{
    class Table
    {
        private readonly string _name;
        private readonly string _schema;
        private readonly DatabaseSchema _databaseSchema;
        private readonly Lazy<ColumnCollection> _lazyColumns;

        public Table(string name, string schema, DatabaseSchema databaseSchema)
        {
            _name = name;
            _databaseSchema = databaseSchema;
            _schema = schema;
            _lazyColumns = new Lazy<ColumnCollection>(GetColumns);
        }

        public DatabaseSchema DatabaseSchema
        {
            get { return _databaseSchema; }
        }

        public string Schema
        {
            get { return _schema; }
        }

        public string Name
        {
            get { return _name; }
        }

        public IEnumerable<Column> Columns
        {
            get { return _lazyColumns.Value.AsEnumerable(); }
        }

        public Column FindColumn(string columnName)
        {
            return _lazyColumns.Value.Find(columnName);
        }

        private ColumnCollection GetColumns()
        {
            return new ColumnCollection(Column.GetColumnsForTable(this));
        }
    }
}
