using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Net;
using Simple.NExtLib.Xml;

namespace Simple.Azure
{
    [Serializable]
    public class TableServiceException : Exception
    {
        private readonly string _code;

        public static TableServiceException CreateFromWebException(WebException ex)
        {
            var xml = GetResponseBodyXml(ex.Response);
            if (xml == null) return new TableServiceException(ex);
            return new TableServiceException(xml["message"].Value, xml["code"].Value, ex);
        }

        public TableServiceException(string message)
            : base(message)
        {
        }

        public TableServiceException(string message, string code)
            : base(message)
        {
            _code = code;
        }

        public TableServiceException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public TableServiceException(string message, string code, Exception inner)
            : base(message, inner)
        {
            _code = code;
        }

        public TableServiceException(WebException inner)
            : base("Unexpected WebException encountered", inner)
        {

        }

        protected TableServiceException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }

        public string Code
        {
            get { return _code; }
        }

        private static XmlElementAsDictionary GetResponseBodyXml(WebResponse response)
        {
            if (response == null) return null;

            var stream = response.GetResponseStream();
            if (stream == null || !stream.CanRead) return null;

            return XmlElementAsDictionary.Parse(stream);
        }
    }
}
