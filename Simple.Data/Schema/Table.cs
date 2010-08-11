using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.Schema
{
    class Table
    {
        private readonly string _actualName;
        private readonly string _homogenizedName;
        private readonly string _schema;
        private readonly DatabaseSchema _databaseSchema;
        private readonly Lazy<ColumnCollection> _lazyColumns;

        public Table(string name, string schema, DatabaseSchema databaseSchema)
        {
            _actualName = name;
            _homogenizedName = name.Homogenize();
            _databaseSchema = databaseSchema;
            _schema = schema;
            _lazyColumns = new Lazy<ColumnCollection>(GetColumns);
        }

        public string HomogenizedName
        {
            get { return _homogenizedName; }
        }

        public DatabaseSchema DatabaseSchema
        {
            get { return _databaseSchema; }
        }

        public string Schema
        {
            get { return _schema; }
        }

        public string ActualName
        {
            get { return _actualName; }
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
