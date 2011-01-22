using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Simple.Data.Extensions;

namespace Simple.Data.Ado.Schema
{
    public class Table : IEquatable<Table>
    {
        private readonly string _actualName;
        private readonly string _schema;
        private readonly TableType _type;
        private readonly DatabaseSchema _databaseSchema;
        private readonly Lazy<ColumnCollection> _lazyColumns;
        private readonly Lazy<Key> _lazyPrimaryKey;
        private readonly Lazy<ForeignKeyCollection> _lazyForeignKeys;

        public Table(string name, string schema, TableType type)
        {
            _actualName = name;
            _schema = string.IsNullOrWhiteSpace(schema) ? null : schema;
            _type = type;
        }

        internal Table(string name, string schema, TableType type, DatabaseSchema databaseSchema)
        {
            _actualName = name;
            _databaseSchema = databaseSchema;
            _schema = schema;
            _type = type;
            _lazyColumns = new Lazy<ColumnCollection>(GetColumns);
            _lazyPrimaryKey = new Lazy<Key>(GetPrimaryKey);
            _lazyForeignKeys = new Lazy<ForeignKeyCollection>(GetForeignKeys);
        }

        public TableType Type
        {
            get { return _type; }
        }

        internal string HomogenizedName
        {
            get { return ActualName.Homogenize(); }
        }

        internal DatabaseSchema DatabaseSchema
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

        internal string QuotedName
        {
            get { return _databaseSchema.QuoteObjectName(_actualName); }
        }

        internal string QualifiedName
        {
            get
            {
                return _schema == null
                           ? _databaseSchema.QuoteObjectName(_actualName)
                           : _databaseSchema.QuoteObjectName(_schema) + '.' + _databaseSchema.QuoteObjectName(_actualName);
            }
        }

        internal IEnumerable<Column> Columns
        {
            get { return _lazyColumns.Value.AsEnumerable(); }
        }

        internal Column FindColumn(string columnName)
        {
            var columns = _lazyColumns.Value;
            try
            {
                return columns.Find(columnName);
            }
            catch (UnresolvableObjectException ex)
            {
                throw new UnresolvableObjectException(_actualName + "." + ex.ObjectName, "Column not found", ex);
            }
        }

        internal Key PrimaryKey
        {
            get { return _lazyPrimaryKey.Value; }
        }

        internal ForeignKeyCollection ForeignKeys
        {
            get { return _lazyForeignKeys.Value; }
        }

        private ColumnCollection GetColumns()
        {
            return new ColumnCollection(_databaseSchema.SchemaProvider.GetColumns(this));
        }

        private Key GetPrimaryKey()
        {
            return _databaseSchema.SchemaProvider.GetPrimaryKey(this);
            //var columns = _databaseSchema.SchemaProvider.GetSchema("PRIMARY_KEYS", ActualName).AsEnumerable()
            //    .OrderBy(row => (int) row["ORDINAL_POSITION"])
            //    .Select(row => row["COLUMN_NAME"].ToString())
            //    .ToArray();

            //return new Key(columns);
        }

        private ForeignKeyCollection GetForeignKeys()
        {
            var collection = new ForeignKeyCollection();

            //var keys = _databaseSchema.SchemaProvider.GetSchema("FOREIGN_KEYS", ActualName).AsEnumerable()
            //    .GroupBy(row => row["UNIQUE_TABLE_NAME"].ToString());

            foreach (var key in _databaseSchema.SchemaProvider.GetForeignKeys(this))
            {
                //var columns = key.OrderBy(row => (int)row["ORDINAL_POSITION"]).Select(row => row["COLUMN_NAME"].ToString()).ToArray();
                //var uniqueColumns = key.OrderBy(row => (int)row["ORDINAL_POSITION"]).Select(row => row["UNIQUE_COLUMN_NAME"].ToString()).ToArray();
                collection.Add(key);
            }

            return collection;
        }

        internal TableJoin GetMaster(string name)
        {
            var table = _databaseSchema.FindTable(name);

            var foreignKey =
                this.ForeignKeys.SingleOrDefault(fk => fk.MasterTable.Schema == table.Schema && fk.MasterTable.Name == table.ActualName);

            if (foreignKey == null) return null;

            return new TableJoin(table, table.FindColumn(foreignKey.UniqueColumns[0]), this, this.FindColumn(foreignKey.Columns[0]));
        }

        private string GetCommonColumnName(Table other)
        {
            return other.Columns
                .Select(c => c.HomogenizedName)
                .Intersect(Columns.Select(c => c.HomogenizedName))
                .SingleOrDefault();
        }

        internal TableJoin GetDetail(string name)
        {
            var table = DatabaseSchema.FindTable(name);
            var foreignKey =
                table.ForeignKeys.SingleOrDefault(fk => fk.MasterTable.Schema == this.Schema && fk.MasterTable.Name == this.ActualName);

            if (foreignKey == null) return null;

            return new TableJoin(this, this.FindColumn(foreignKey.UniqueColumns[0]), table, table.FindColumn(foreignKey.Columns[0]));
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(Table other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._actualName, _actualName) && Equals(other._schema, _schema) && Equals(other._type, _type);
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>. </param><filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (Table)) return false;
            return Equals((Table) obj);
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = (_actualName != null ? _actualName.GetHashCode() : 0);
                result = (result*397) ^ (_schema != null ? _schema.GetHashCode() : 0);
                result = (result*397) ^ _type.GetHashCode();
                return result;
            }
        }
    }
}
