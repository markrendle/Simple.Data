using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Simple.Data
{
    public class DynamicColumn : IEquatable<DynamicColumn>
    {
        private readonly string _tableName;
        private readonly string _name;

        internal DynamicColumn(string tableName, string name)
        {
            _name = name;
            _tableName = tableName;
        }

        public string TableName
        {
            get { return _tableName; }
        }

        public string Name
        {
            get { return _name; }
        }

        public static SimpleExpression operator ==(DynamicColumn column, object value)
        {
            return new SimpleExpression(column, value, SimpleExpressionType.Equal);
        }

        public static SimpleExpression operator !=(DynamicColumn column, object value)
        {
            return new SimpleExpression(column, value, SimpleExpressionType.NotEqual);
        }

        public static SimpleExpression operator <(DynamicColumn column, object value)
        {
            return new SimpleExpression(column, value, SimpleExpressionType.LessThan);
        }

        public static SimpleExpression operator >(DynamicColumn column, object value)
        {
            return new SimpleExpression(column, value, SimpleExpressionType.GreaterThan);
        }

        public static SimpleExpression operator <=(DynamicColumn column, object value)
        {
            return new SimpleExpression(column, value, SimpleExpressionType.LessThanOrEqual);
        }

        public static SimpleExpression operator >=(DynamicColumn column, object value)
        {
            return new SimpleExpression(column, value, SimpleExpressionType.GreaterThanOrEqual);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(DynamicColumn other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._tableName, _tableName) && Equals(other._name, _name);
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
            if (obj.GetType() != typeof (DynamicColumn)) return false;
            return Equals((DynamicColumn) obj);
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
                return ((_tableName != null ? _tableName.GetHashCode() : 0)*397) ^ (_name != null ? _name.GetHashCode() : 0);
            }
        }
    }
}
