using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Simple.Data.Extensions;

namespace Simple.Data.Ado
{
    public class AdoAdapterException : AdapterException
    {
        private readonly string _commandText;
        private readonly IDictionary<string,object> _parameters;

        public AdoAdapterException() : base(typeof(AdoAdapter))
        {
        }

        public AdoAdapterException(string message, IDbCommand command) : base(message, typeof(AdoAdapter))
        {
            _commandText = command.CommandText;
            _parameters = command.Parameters.Cast<IDbDataParameter>()
                .ToDictionary(p => p.ParameterName, p => p.Value);
        }

        public AdoAdapterException(string commandText, IEnumerable<KeyValuePair<string,object>> parameters)
            :base(typeof(AdoAdapter))
        {
            _commandText = commandText;
            _parameters = parameters.ToDictionary();
        }


        public AdoAdapterException(string message) : base(message, typeof(AdoAdapter))
        {
        }

        public AdoAdapterException(string message, string commandText, IEnumerable<KeyValuePair<string,object>> parameters)
            :base(message, typeof(AdoAdapter))
        {
            _commandText = commandText;
            _parameters = parameters.ToDictionary();
        }

        public AdoAdapterException(string message, Exception inner) : base(message, inner, typeof(AdoAdapter))
        {
        }

        public AdoAdapterException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public IDictionary<string, object> Parameters
        {
            get { return _parameters; }
        }

        public string CommandText
        {
            get { return _commandText; }
        }
    }
}
