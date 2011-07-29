using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Simple.Data.Extensions;

namespace Simple.Data.Ado.Schema
{
    public class Procedure
    {
        private readonly DatabaseSchema _databaseSchema;
        private readonly string _name;
        private readonly string _specificName;
        private readonly string _schema;
        private readonly Lazy<ParameterCollection> _lazyParameters;

        public Procedure(string name, string specificName, string schema)
        {
            _name = name;
            _specificName = specificName;
            _schema = schema.NullIfWhitespace();
            _lazyParameters = new Lazy<ParameterCollection>(() => new ParameterCollection(GetParameters()));
         }

        internal Procedure(string name, string specificName, string schema, DatabaseSchema databaseSchema)
        {
            _name = name;
            _specificName = specificName;
            _schema = schema.NullIfWhitespace();
            _lazyParameters = new Lazy<ParameterCollection>(() => new ParameterCollection(GetParameters()));
            _databaseSchema = databaseSchema;
        }

        private IEnumerable<Parameter> GetParameters()
        {
            return _databaseSchema.SchemaProvider.GetParameters(this);
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

        public ParameterCollection Parameters
        {
            get { return _lazyParameters.Value; }
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

        internal string QualifiedName
        {
            get { return string.IsNullOrWhiteSpace(_schema) ? QuotedName : string.Format("{0}.{1}", _databaseSchema.QuoteObjectName(_schema), _databaseSchema.QuoteObjectName(_name)); }
        }
    }

    public class ParameterCollection : Collection<Parameter>
    {
        public ParameterCollection(IEnumerable<Parameter> parameters)
        {
            foreach (var parameter in parameters)
            {
                Add(parameter);
            }
        }
    }
}
