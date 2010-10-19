using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace Simple.Data
{
    public class DynamicReference : DynamicObject, IEquatable<DynamicReference>
    {
        private readonly string _name;
        private readonly DynamicReference _owner;

        internal DynamicReference(string name) : this(name, null)
        {
        }

        internal DynamicReference(string name, DynamicReference owner)
        {
            _name = name;
            _owner = owner;
        }

        public DynamicReference Owner
        {
            get { return _owner; }
        }

        public string Name
        {
            get { return _name; }
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = new DynamicReference(binder.Name, this);
            return true;
        }

        public string[] GetAllObjectNames()
        {
            if (ReferenceEquals(Owner, null)) return new[] {_name};
            return _owner.GetAllObjectNames().Concat(new[] {_name}).ToArray();
        }

        public static DynamicReference FromString(string source)
        {
            return FromStrings(source.Split('.'));
        }

        public static DynamicReference FromStrings(params string[] source)
        {
            return source.Aggregate<string, DynamicReference>(null, (current, element) => new DynamicReference(element, current));
        }

        public static SimpleExpression operator ==(DynamicReference column, object value)
        {
            return new SimpleExpression(column, value, SimpleExpressionType.Equal);
        }

        public static SimpleExpression operator !=(DynamicReference column, object value)
        {
            return new SimpleExpression(column, value, SimpleExpressionType.NotEqual);
        }

        public static SimpleExpression operator <(DynamicReference column, object value)
        {
            return new SimpleExpression(column, value, SimpleExpressionType.LessThan);
        }

        public static SimpleExpression operator >(DynamicReference column, object value)
        {
            return new SimpleExpression(column, value, SimpleExpressionType.GreaterThan);
        }

        public static SimpleExpression operator <=(DynamicReference column, object value)
        {
            return new SimpleExpression(column, value, SimpleExpressionType.LessThanOrEqual);
        }

        public static SimpleExpression operator >=(DynamicReference column, object value)
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
        public bool Equals(DynamicReference other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._name, _name) && Equals(other._owner, _owner);
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
            if (obj.GetType() != typeof (DynamicReference)) return false;
            return Equals((DynamicReference) obj);
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
                return ((_name != null ? _name.GetHashCode() : 0)*397) ^ (!ReferenceEquals(_owner, null) ? _owner.GetHashCode() : 0);
            }
        }
    }
}
