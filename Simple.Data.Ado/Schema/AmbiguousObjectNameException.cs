using System;
using System.Runtime.Serialization;

namespace Shitty.Data.Ado.Schema
{
    using System.Security;

    [Serializable]
    [SecurityCritical]
    public class AmbiguousObjectNameException : Exception
    {
        public AmbiguousObjectNameException()
        {
        }

        public AmbiguousObjectNameException(string name)
        {
            Name = name;
        }

        public AmbiguousObjectNameException(string name, string message) : base(message)
        {
            Name = name;
        }

        public AmbiguousObjectNameException(string name, string message, Exception inner) : base(message, inner)
        {
            Name = name;
        }

        protected AmbiguousObjectNameException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }

        public string Name
        {
            get { return Data.Contains("Name") ?  Data["Name"].ToString() : null; }
            private set { Data["Name"] = value; }
        }

        //[SecurityCritical]
        //public override void GetObjectData(SerializationInfo info, StreamingContext context)
        //{
        //    base.GetObjectData(info, context);
        //    info.AddValue("Name", Name);
        //}
    }
}
