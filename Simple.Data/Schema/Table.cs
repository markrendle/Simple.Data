using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.Schema
{
    class Table
    {
        private readonly string _name;
        private readonly string _schema;

        public Table(string name, string schema)
        {
            _name = name;
            _schema = schema;
        }

        public string Schema
        {
            get { return _schema; }
        }

        public string Name
        {
            get { return _name; }
        }
    }
}
