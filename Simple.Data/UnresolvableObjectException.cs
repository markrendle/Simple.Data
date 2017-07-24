using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Shitty.Data
{
    using System.Security;

    [Serializable]
    public sealed class UnresolvableObjectException : Exception
    {
        public UnresolvableObjectException()
        {
        }

        public UnresolvableObjectException(string objectName)
        {
            ObjectName = objectName;
        }

        public UnresolvableObjectException(string objectName, string message) : base(message)
        {
            ObjectName = objectName;
        }

        public UnresolvableObjectException(string objectName, string message, Exception inner) : base(message, inner)
        {
            ObjectName = objectName;
        }

        private UnresolvableObjectException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
            ObjectName = info.GetString("ObjectName");
        }

        public string ObjectName
        {
            get { return Data.Contains("ObjectName") ? Data["ObjectName"].ToString() : null; }
            private set { Data["ObjectName"] = value; }
        }
    }
}
