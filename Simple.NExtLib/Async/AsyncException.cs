using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.NExtLib.Async
{
    [Serializable]
    public class AsyncException : Exception
    {
        public AsyncException() { }
        public AsyncException(string message) : base(message) { }
        public AsyncException(string message, Exception inner) : base(message, inner) { }
        protected AsyncException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
