using System;
using System.Data;

namespace Simple.Data.Ado
{
    using Schema;

    public class ParameterTemplate : IEquatable<ParameterTemplate>
    {
        private readonly object _fixedValue;
        private readonly string _name;
        private readonly DbType _dbType;
        private readonly int _maxLength;
        private readonly Column _column;
        private readonly ParameterType _type;

        public ParameterTemplate(string name, object fixedValue) : this(name, null)
        {
            _fixedValue = fixedValue;
            _type = ParameterType.FixedValue;
        }

        public ParameterTemplate(string name, Column column)
        {
            if (name == null) throw new ArgumentNullException("name");
            _name = name;
            _column = column;
            if (column == null)
            {
                _type = ParameterType.NameOnly;
                return;
            }
            _type = ParameterType.Column;
            _dbType = column.DbType;
            _maxLength = column.MaxLength;
        }

        public ParameterTemplate(string name, DbType dbType, int maxLength)
        {
            if (name == null) throw new ArgumentNullException("name");
            _name = name;
            _dbType = dbType;
            _maxLength = maxLength;
            _type = ParameterType.Other;
        }

        internal ParameterType Type
        {
            get { return _type; }
        }

        public int MaxLength
        {
            get { return _maxLength; }
        }

        public DbType DbType
        {
            get { return _dbType; }
        }

        public string Name
        {
            get { return _name; }
        }

        public Column Column
        {
            get { return _column; }
        }

        public object FixedValue
        {
            get {
                return _fixedValue;
            }
        }

        public ParameterTemplate Rename(string newName)
        {
            return _column == null
                       ? new ParameterTemplate(newName, _dbType, _maxLength)
                       : new ParameterTemplate(newName, _column);
        }

        public bool Equals(ParameterTemplate other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._name, _name) && Equals(other._dbType, _dbType);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (ParameterTemplate)) return false;
            return Equals((ParameterTemplate) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_name.GetHashCode()*397) ^ _dbType.GetHashCode();
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }

    enum ParameterType
    {
        Column,
        FixedValue,
        Other,
        NameOnly
    }
}