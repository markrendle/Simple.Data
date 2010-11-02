using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Simple.Data
{
    [Serializable]
    public abstract class AdapterException : Exception
    {
        private readonly Type _adapterType;

        protected AdapterException(Type adapterType)
        {
            _adapterType = adapterType;
        }

        protected AdapterException(string message, Type adapterType) : base(message)
        {
            _adapterType = adapterType;
        }

        protected AdapterException(string message, Exception inner, Type adapterType) : base(message, inner)
        {
            _adapterType = adapterType;
        }

        protected AdapterException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
            _adapterType = info.GetValue("AdapterType", typeof(Type)) as Type;
        }

        public Type AdapterType
        {
            get { return _adapterType; }
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("AdapterType", _adapterType);
        }
    }
}
