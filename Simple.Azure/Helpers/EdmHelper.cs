using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Xml.Linq;
using System.Diagnostics;
using Simple.NExtLib;

namespace Simple.Azure.Helpers
{
    public static class EdmHelper
    {
        private static readonly Dictionary<EdmType, Func<string, object>> Readers = new Dictionary<EdmType, Func<string, object>>
        {
            { EdmType.Binary, ReadEdmBinary },
            { EdmType.Boolean, ReadEdmBoolean },
            { EdmType.DateTime, ReadEdmDateTime },
            { EdmType.Double, ReadEdmDouble },
            { EdmType.Guid, ReadEdmGuid },
            { EdmType.Int32, ReadEdmInt32 },
            { EdmType.Int64, ReadEdmInt64 },
        };

        private static readonly Dictionary<EdmType, Func<object, string>> Writers = new Dictionary<EdmType, Func<object, string>>
        {
            { EdmType.Binary, WriteEdmBinary },
            { EdmType.Boolean, WriteEdmBoolean },
            { EdmType.DateTime, WriteEdmDateTime },
            { EdmType.Double, WriteEdmDouble },
            { EdmType.Guid, WriteEdmGuid },
            { EdmType.Int32, WriteEdmInt32 },
            { EdmType.Int64, WriteEdmInt64 },
            { EdmType.String, WriteEdmString },
        };

        public static KeyValuePair<string, object> Read(XElement element)
        {
            if (element == null) throw new ArgumentNullException("element");

            if (element.Attribute("m", "null").ValueOrDefault() == "true")
            {
                return new KeyValuePair<string, object>(element.Name.LocalName, null);
            }

            var reader = GetReader(element.Attribute("m", "type").ValueOrDefault());

            return new KeyValuePair<string, object>(element.Name.LocalName, reader(element.Value));
        }

        public static void Write(XElement container, KeyValuePair<string, object> kvp)
        {
            var element = new XElement(container.GetNamespaceOfPrefix("d") + kvp.Key);;

            if (kvp.Value == null)
            {
                element.SetAttributeValue(container.GetNamespaceOfPrefix("m") + "null", "true");
            }
            else
            {
                var type = EdmType.FromSystemType(kvp.Value.GetType());
                if (type != EdmType.String)
                {
                    element.SetAttributeValue(container.GetNamespaceOfPrefix("m") + "type", type.ToString());
                }
                element.SetValue(Writers[type](kvp.Value));
            }

            container.Add(element);
        }

        public static object ReadEdmBinary(string source)
        {
            return Convert.FromBase64String(source);
        }

        public static object ReadEdmBoolean(string source)
        {
            return source.Equals("true", StringComparison.InvariantCultureIgnoreCase);
        }

        public static object ReadEdmDateTime(string source)
        {
            return DateTime.Parse(source);
        }

        public static object ReadEdmDouble(string source)
        {
            return double.Parse(source);
        }

        public static object ReadEdmGuid(string source)
        {
            return new Guid(source);
        }

        public static object ReadEdmInt32(string source)
        {
            return Int32.Parse(source);
        }

        public static object ReadEdmInt64(string source)
        {
            return Int64.Parse(source);
        }

        public static string WriteEdmBinary(object source)
        {
            return Convert.ToBase64String((byte[])source);
        }

        public static string WriteEdmBoolean(object source)
        {
            return (bool)source ? "true" : "false";
        }

        public static string WriteEdmDateTime(object source)
        {
            if (((DateTime)source).Kind != DateTimeKind.Utc)
            {
                Trace.WriteLine("Non-UTC DateTime specified to EdmHelper", "AzureKit.Warnings");
            }

            return ((DateTime)source).ToIso8601String();
        }

        public static string WriteEdmDouble(object source)
        {
            return ((double)source).ToString();
        }

        public static string WriteEdmGuid(object source)
        {
            return ((Guid)source).ToString();
        }

        public static string WriteEdmInt32(object source)
        {
            return ((Int32)source).ToString();
        }

        public static string WriteEdmInt64(object source)
        {
            return ((Int64)source).ToString();
        }

        public static string WriteEdmString(object source)
        {
            return source.ToString();
        }

        private static Func<string, object> GetReader(string edmType)
        {
            var func = Func.NoOp<string, object>();

            EdmType.TryParse(edmType).IfGood((et) =>
                {
                    func = Readers[et];
                });

            return func;
        }
    }
}
