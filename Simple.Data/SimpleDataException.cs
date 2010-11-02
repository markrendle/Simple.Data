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
        public SimpleDataException()
        {
        }

        public SimpleDataException(string message) : base(message)
        {
        }

        public SimpleDataException(string message, Exception inner) : base(message, inner)
        {
        }

        protected SimpleDataException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
