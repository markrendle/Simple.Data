using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Simple.Data.Schema
{
    class Table
    {
        private readonly string _actualName;
        private readonly string _homogenizedName;
        private readonly string _schema;
        private readonly TableType _type;
        private readonly DatabaseSchema _databaseSchema;
        private readonly Lazy<ColumnCollection> _lazyColumns;
        private readonly Lazy<Key> _lazyPrimaryKey;
        private readonly Lazy<ForeignKeyCollection> _lazyForeignKeys;

        public Table(string name, string schema, string type, DatabaseSchema databaseSchema)
        {
            _actualName = name;
            _homogenizedName = name.Homogenize();
            _databaseSchema = databaseSchema;
            _schema = schema;
            _type = type.Equals("BASE TABLE", StringComparison.InvariantCultureIgnoreCase)
                        ? TableType.Table
                        : TableType.View;
            _lazyColumns = new Lazy<ColumnCollection>(GetColumns);
            _lazyPrimaryKey = new Lazy<Key>(GetPrimaryKey);
            _lazyForeignKeys = new Lazy<ForeignKeyCollection>(GetForeignKeys);
        }

        public TableType Type
        {
            get { return _type; }
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
            var columns = _lazyColumns.Value;
            return columns.Find(columnName);
        }

        public Key PrimaryKey
        {
            get { return _lazyPrimaryKey.Value; }
        }

        public ForeignKeyCollection ForeignKeys
        {
            get { return _lazyForeignKeys.Value; }
        }

        private ColumnCollection GetColumns()
        {
            return new ColumnCollection(Column.GetColumnsForTable(this));
        }

        private Key GetPrimaryKey()
        {
            var columns = _databaseSchema.SchemaProvider.GetSchema("PRIMARY_KEYS", ActualName).AsEnumerable()
                .OrderBy(row => (int) row["ORDINAL_POSITION"])
                .Select(row => row["COLUMN_NAME"].ToString())
                .ToArray();

            return new Key(columns);
        }

        private ForeignKeyCollection GetForeignKeys()
        {
            var collection = new ForeignKeyCollection();

            var keys = _databaseSchema.SchemaProvider.GetSchema("FOREIGN_KEYS", ActualName).AsEnumerable()
                .GroupBy(row => row["UNIQUE_TABLE_NAME"].ToString());

            foreach (var key in keys)
            {
                var columns = key.OrderBy(row => (int)row["ORDINAL_POSITION"]).Select(row => row["COLUMN_NAME"].ToString()).ToArray();
                var uniqueColumns = key.OrderBy(row => (int)row["ORDINAL_POSITION"]).Select(row => row["UNIQUE_COLUMN_NAME"].ToString()).ToArray();
                collection.Add(new ForeignKey(ActualName, columns, key.Key, uniqueColumns));
            }

            return collection;
        }

        public TableJoin GetMaster(string name)
        {
            var master = DatabaseSchema.FindTable(name);
            if (master != null)
            {
                string commonColumnName = GetCommonColumnName(master);

                if (commonColumnName != null)
                {
                    return new TableJoin(master, master.FindColumn(commonColumnName), this, FindColumn(commonColumnName));
                }
            }
            return null;
        }

        private string GetCommonColumnName(Table other)
        {
            return other.Columns
                .Select(c => c.HomogenizedName)
                .Intersect(this.Columns.Select(c => c.HomogenizedName))
                .SingleOrDefault();
        }

        public TableJoin GetDetail(string name)
        {
            var detail = DatabaseSchema.FindTable(name);
            string commonColumnName = GetCommonColumnName(detail);
            if (detail.Columns.Select(c => c.HomogenizedName).Intersect(this.Columns.Select(c => c.HomogenizedName)).Count() == 1)
            {
                return new TableJoin(this, FindColumn(commonColumnName), detail, detail.FindColumn(commonColumnName));
            }
            return null;
        }
    }
}
