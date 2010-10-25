using System;
using System.Runtime.Serialization;

namespace Simple.Data.Ado.Schema
{
    [Serializable]
    public class AmbiguousObjectNameException : Exception
    {
        private readonly string _name;

        public AmbiguousObjectNameException()
        {
        }

        public AmbiguousObjectNameException(string name)
        {
            _name = name;
        }

        public AmbiguousObjectNameException(string name, string message) : base(message)
        {
            _name = name;
        }

        public AmbiguousObjectNameException(string name, string message, Exception inner) : base(message, inner)
        {
            _name = name;
        }

        protected AmbiguousObjectNameException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
            _name = info.GetString("_name");
        }

        public string Name
        {
            get { return _name; }
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("_name", _name);
        }
    }
}
