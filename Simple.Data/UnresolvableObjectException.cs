using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Simple.Data
{
    [Serializable]
    public class UnresolvableObjectException : Exception
    {
        private readonly string _objectName;

        public UnresolvableObjectException()
        {
        }

        public UnresolvableObjectException(string objectName)
        {
            _objectName = objectName;
        }

        public UnresolvableObjectException(string objectName, string message) : base(message)
        {
            _objectName = objectName;
        }

        public UnresolvableObjectException(string objectName, string message, Exception inner) : base(message, inner)
        {
            _objectName = objectName;
        }

        protected UnresolvableObjectException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
            _objectName = info.GetString("_objectName");
        }

        public string ObjectName
        {
            get { return _objectName; }
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("_objectName", _objectName);
        }
    }
}
