using System;
using System.Collections.Generic;
using System.Data;
using Simple.Data.Extensions;

namespace Simple.Data.Ado.Schema
{
    public class Column : IEquatable<Column>
    {
        private readonly string _actualName;
        private readonly Table _table;
        private readonly bool _isIdentity;
        private readonly DbType _dbType;
        private readonly int _maxLength;

        public Column(string actualName, Table table) : this(actualName, table, DbType.Object)
        {
        }

        public Column(string actualName, Table table, DbType dbType) : this(actualName, table, false, dbType, 0)
        {
        }

        public Column(string actualName, Table table, bool isIdentity) : this(actualName, table, isIdentity, DbType.Object, 0)
        {
        }

        public Column(string actualName, Table table, bool isIdentity, DbType dbType, int maxLength)
        {
            _actualName = actualName;
            _dbType = dbType;
            _maxLength = maxLength;
            _isIdentity = isIdentity;
            _table = table;
        }

        public int MaxLength
        {
            get { return _maxLength; }
        }

        public DbType DbType
        {
            get { return _dbType; }
        }

        public string HomogenizedName
        {
            get { return ActualName.Homogenize(); }
        }

        public string ActualName
        {
            get { return _actualName; }
        }

        public string QuotedName
        {
            get { return _table.DatabaseSchema.QuoteObjectName(_actualName); }
        }

        public bool IsIdentity
        {
            get { return _isIdentity; }
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(Column other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._actualName, _actualName) && Equals(other._table, _table);
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
            if (obj.GetType() != typeof (Column)) return false;
            return Equals((Column) obj);
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
                return ((_actualName != null ? _actualName.GetHashCode() : 0)*397) ^ (_table != null ? _table.GetHashCode() : 0);
            }
        }
    }
}
