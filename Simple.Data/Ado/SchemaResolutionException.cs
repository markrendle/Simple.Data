using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Simple.Data.Ado
{
    [Serializable]
    public class SchemaResolutionException : SimpleDataException
    {
        public SchemaResolutionException() : base(typeof(AdoAdapter))
        {
        }

        public SchemaResolutionException(string message)
            : base(message, typeof(AdoAdapter))
        {
        }

        public SchemaResolutionException(string message, Exception inner)
            : base(message, inner, typeof(AdoAdapter))
        {
        }

        protected SchemaResolutionException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
