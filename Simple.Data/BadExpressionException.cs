using System;
using System.Runtime.Serialization;

namespace Simple.Data
{
    [Serializable]
    public class BadExpressionException : ArgumentException
    {
        public BadExpressionException()
        {}

        public BadExpressionException(string message) : base(message)
        {}

        public BadExpressionException(string message, Exception inner) : base(message, inner)
        {}

        protected BadExpressionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {}
    }
    
    [Serializable]
    public class BadJoinExpressionException : BadExpressionException
    {
        public BadJoinExpressionException()
        {}

        public BadJoinExpressionException(string message) : base(message)
        {}

        public BadJoinExpressionException(string message, Exception inner) : base(message, inner)
        {}

        protected BadJoinExpressionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {}
    }
}