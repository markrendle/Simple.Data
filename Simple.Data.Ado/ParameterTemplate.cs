using System;
using System.Data;

namespace Simple.Data.Ado
{
    public class ParameterTemplate : IEquatable<ParameterTemplate>
    {
        private readonly string _name;
        private readonly DbType _dbType;
        private readonly int _maxLength;

        public ParameterTemplate(string name) : this(name, DbType.Object, 0)
        {
        }

        public ParameterTemplate(string name, DbType dbType, int maxLength)
        {
            if (name == null) throw new ArgumentNullException("name");
            _name = name;
            _dbType = dbType;
            _maxLength = maxLength;
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
}