using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Simple.Data.Extensions;

namespace Simple.Data.Ado
{
    using System.Security;

    [Serializable]
    public class AdoAdapterException : AdapterException
    {
        public AdoAdapterException() : base(typeof(AdoAdapter))
        {
        }

        public AdoAdapterException(string message, IDbCommand command) : base(message, typeof(AdoAdapter))
        {
            CommandText = command.CommandText;
            Parameters = command.Parameters.Cast<IDbDataParameter>()
                .ToDictionary(p => p.ParameterName, p => p.Value);
        }

        public AdoAdapterException(string commandText, IEnumerable<KeyValuePair<string,object>> parameters)
            :base(typeof(AdoAdapter))
        {
            CommandText = commandText;
            Parameters = parameters.ToDictionary();
        }


        public AdoAdapterException(string message) : base(message, typeof(AdoAdapter))
        {
        }

        public AdoAdapterException(string message, string commandText, IEnumerable<KeyValuePair<string,object>> parameters)
            :base(message, typeof(AdoAdapter))
        {
            CommandText = commandText;
            Parameters = parameters.ToDictionary();
        }

        public AdoAdapterException(string message, Exception inner) : base(message, inner, typeof(AdoAdapter))
        {
        }

        protected AdoAdapterException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            //CommandText = info.GetString("CommandText");
            //try
            //{
            //    var array = info.GetValue("Parameters", typeof(KeyValuePair<string, object>[]));
            //    if (array != null)
            //    {
            //        Parameters = ((KeyValuePair<string, object>[])array);
            //    }
            //}
            //catch (SerializationException)
            //{
            //}
        }

        public IDictionary<string, object> Parameters
        {
            get { return Data.Contains("Parameters") ?  ((KeyValuePair<string,object>[])Data["Parameters"]).ToDictionary() : null; }
            private set { Data["Parameters"] = value.ToArray(); }
        }

        public string CommandText
        {
            get { return Data.Contains("CommandText") ?  Data["CommandText"].ToString() : null; }
            private set { Data["CommandText"] = value; }
        }
    }
}
