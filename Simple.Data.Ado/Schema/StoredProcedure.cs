using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Simple.Data.Extensions;

namespace Simple.Data.Ado.Schema
{
    public class StoredProcedure
    {
        private readonly DatabaseSchema _databaseSchema;
        private readonly string _name;
        private readonly string _specificName;
        private readonly string _schema;

        public StoredProcedure(string name, string specificName, string schema)
        {
            _name = name;
            _specificName = specificName;
            _schema = schema.NullIfWhitespace();
         }

        internal StoredProcedure(string name, string specificName, string schema, DatabaseSchema databaseSchema)
        {
            _name = name;
            _specificName = specificName;
            _schema = schema.NullIfWhitespace();
            _databaseSchema = databaseSchema;
        }

        public string SpecificName
        {
            get { return _specificName; }
        }

        public string Schema
        {
            get { return _schema; }
        }

        public string Name
        {
            get { return _name; }
        }

        internal string HomogenizedName
        {
            get { return Name.Homogenize(); }
        }

        internal string HomogenizedSpecificName
        {
            get { return SpecificName.Homogenize(); }
        }

        internal string QuotedName
        {
            get { return _databaseSchema.QuoteObjectName(_name); }
        }
    }
}
