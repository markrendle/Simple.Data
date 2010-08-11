using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Simple.Data.Schema
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
        }

        public string Name
        {
            get { return _name; }
        }
    }
}
