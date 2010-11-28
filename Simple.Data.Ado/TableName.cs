using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.Ado
{
    public class TableName : IEquatable<TableName>
    {
        private readonly string _schema;
        private readonly string _table;

        public TableName(string schema, string table)
        {
            if (schema == null) throw new ArgumentNullException("schema");
            if (table == null) throw new ArgumentNullException("table");
            _schema = schema;
            _table = table;
        }

        public static TableName Parse(string text)
        {
            if (text == null) throw new ArgumentNullException("text");
            if (!text.Contains('.')) return new TableName(Properties.Settings.Default.DefaultSchema ?? "dbo", text);
            var schemaDotTable = text.Split('.');
            if (schemaDotTable.Length != 2) throw new InvalidOperationException("Could not parse table name.");
            return new TableName(schemaDotTable[0], schemaDotTable[1]);
        }

        public string Table
        {
            get { return _table; }
        }

        public string Schema
        {
            get { return _schema; }
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(TableName other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._schema, _schema) && Equals(other._table, _table);
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
            if (obj.GetType() != typeof (TableName)) return false;
            return Equals((TableName) obj);
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
                return (_schema.GetHashCode()*397) ^ _table.GetHashCode();
            }
        }

        public static bool operator ==(TableName left, TableName right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(TableName left, TableName right)
        {
            return !Equals(left, right);
        }
    }
}
