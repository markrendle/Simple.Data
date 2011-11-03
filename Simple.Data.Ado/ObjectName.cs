using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Simple.Data.Ado.Schema;

namespace Simple.Data.Ado
{
    public class ObjectName : IEquatable<ObjectName>
    {
        private readonly string _schema;
        private readonly string _name;

        public ObjectName(object schema, object name)
        {
            if (name == null) throw new ArgumentNullException("name");
            _schema = schema != null && schema != DBNull.Value ? (string)schema : null;
            _name = (string)name;
        }

        public ObjectName(string schema, string name)
        {
            if (name == null) throw new ArgumentNullException("name");
            _schema = schema;
            _name = name;
        }

        public string Name
        {
            get { return _name; }
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
        public bool Equals(ObjectName other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._schema, _schema) && Equals(other._name, _name);
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
            if (obj.GetType() != typeof (ObjectName)) return false;
            return Equals((ObjectName) obj);
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
                return ((_schema??string.Empty).GetHashCode()*397) ^ _name.GetHashCode();
            }
        }

        public static bool operator ==(ObjectName left, ObjectName right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ObjectName left, ObjectName right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            return _schema == null ? _name : _schema + "." + _name;
        }
    }
}
