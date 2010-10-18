using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data
{
    public class DynamicColumn
    {
        private readonly string _name;

        public DynamicColumn(string name)
        {
            _name = name;
        }

        public string Name
        {
            get { return _name; }
        }
    }
}
