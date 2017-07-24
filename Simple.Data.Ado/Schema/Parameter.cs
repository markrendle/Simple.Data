using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Shitty.Data.Ado.Schema
{
    public class Parameter
    {
        private readonly string _name;
        private readonly Type _type;
        private readonly ParameterDirection _direction;
        private int _size;
        private DbType _dbtype;

        public Parameter(string name, Type type, ParameterDirection direction)
        {
            _name = name;
            _direction = direction;
            _type = type;
        }

        public Parameter(string name, Type type, ParameterDirection direction, DbType dbtype, int size)
            : this(name, type, direction)
        {
            _dbtype = dbtype;
            _size = size;
        }

        public ParameterDirection Direction
        {
            get { return _direction; }
        }

        public Type Type
        {
            get { return _type; }
        }

        public string Name
        {
            get { return _name; }
        }
        //Tim Cartwright: I added size and dbtype so inout/out params would function properly.
        public int Size
        {
            get { return _size; }
        }

        public DbType Dbtype
        {
            get { return _dbtype; }
        }

    }
}
