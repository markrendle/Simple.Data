using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Simple.Data.Extensions;

namespace Simple.Data.Ado.Schema
{
    public class StoredProcedure
    {
        private readonly string _name;
        private readonly string _schema;

        public StoredProcedure(string name) : this(name, null)
        {
        }

        public StoredProcedure(string name, string schema)
        {
            _name = name;
            _schema = schema.NullIfWhitespace();
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
