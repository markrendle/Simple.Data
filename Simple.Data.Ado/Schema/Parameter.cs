using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Simple.Data.Ado.Schema
{
    public class Parameter
    {
        private readonly string _name;
        private readonly Type _type;
        private readonly ParameterDirection _direction;

        public Parameter(string name, Type type, ParameterDirection direction)
        {
            _name = name;
            _direction = direction;
            _type = type;
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
    }
}
