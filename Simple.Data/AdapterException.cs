using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Simple.Data
{
    using System.Security;

    [Serializable]
    public abstract class AdapterException : Exception
    {
        protected AdapterException(Type adapterType)
        {
            AdapterType = adapterType;
        }

        protected AdapterException(string message, Type adapterType) : base(message)
        {
            AdapterType = adapterType;
        }

        protected AdapterException(string message, Exception inner, Type adapterType) : base(message, inner)
        {
            AdapterType = adapterType;
        }

        protected AdapterException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
            
            
        }

        public Type AdapterType
        {
            get { return Data.Contains("AdapterType") ? Data["AdapterType"] as Type : null; }
            private set { Data["AdapterType"] = value; }
        }
    }
}
