using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Simple.Data.Extensions;

namespace Simple.Data
{
    [Serializable]
    public class SimpleDataException : Exception
    {
        private readonly Type _adapterType;

        public SimpleDataException() : this(null)
        {
        }

        public SimpleDataException(Type adapterType)
        {
            _adapterType = adapterType;
        }

        public SimpleDataException(string message, Type adapterType) : base(message)
        {
            _adapterType = adapterType;
        }

        public SimpleDataException(string message, Exception inner, Type adapterType) : base(message, inner)
        {
            _adapterType = adapterType;
        }

        protected SimpleDataException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
            _adapterType = info.GetValue<Type>("AdapterType");
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("AdapterType", _adapterType, typeof(Type));
        }

        public Type AdapterType
        {
            get { return _adapterType; }
        }
    }
}
