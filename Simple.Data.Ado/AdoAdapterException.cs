using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Simple.Data.Extensions;

namespace Simple.Data.Ado
{
    [Serializable]
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
            _commandText = info.GetString("_commandText");
            try
            {
                var array = info.GetValue("_parameters", typeof (KeyValuePair<string, object>[]));
                if (array != null)
                {
                    _parameters = ((KeyValuePair<string, object>[]) array).ToDictionary();
                }
            }
            catch (SerializationException)
            {
            }
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("_commandText", _commandText);
            if (_parameters != null)
            {
                info.AddValue("_parameters", _parameters.ToArray(), typeof(KeyValuePair<string,object>[]));
            }
            else
            {
                info.AddValue("_parameters", null);
            }
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
