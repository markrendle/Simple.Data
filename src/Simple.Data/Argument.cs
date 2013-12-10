using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data
{
    public class Argument
    {
        private readonly string _name;
        private readonly object _value;

        public Argument(string name, object value)
        {
            _name = name;
            _value = value;
        }

        public object Value
        {
            get { return _value; }
        }

        public string Name
        {
            get { return _name; }
        }
    }
}
