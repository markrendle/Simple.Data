using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Simple.NExtLib;
using System.Reflection;

namespace Simple.Azure
{
    public sealed class EdmType : IEquatable<EdmType>
    {
        public static readonly EdmType Binary = new EdmType("Edm.Binary");
        public static readonly EdmType Boolean = new EdmType("Edm.Boolean");
        public static readonly EdmType DateTime = new EdmType("Edm.DateTime");
        public static readonly EdmType Double = new EdmType("Edm.Double");
        public static readonly EdmType Guid = new EdmType("Edm.Guid");
        public static readonly EdmType Int32 = new EdmType("Edm.Int32");
        public static readonly EdmType Int64 = new EdmType("Edm.Int64");
        public static readonly EdmType String = new EdmType("Edm.String");

        private static readonly Dictionary<Type, EdmType> EdmTypeMap = new Dictionary<Type, EdmType>
        {
            { typeof(byte[]), Binary },
            { typeof(bool), Boolean },
            { typeof(DateTime), DateTime },
            { typeof(double), Double },
            { typeof(Guid), Guid },
            { typeof(Int32), Int32 },
            { typeof(Int64), Int64 },
        };

        private readonly string _text;

        private EdmType(string text)
        {
            _text = text;
        }

        public static EdmType Parse(string s)
        {
            var result = TryParse(s);

            if (!result.Item1) throw new ArgumentOutOfRangeException();

            return result.Item2;
        }

        public static Tuple<bool, EdmType> TryParse(string s)
        {
            s = s.EnsureStartsWith("Edm.");

            var edmType = EnumerateTypes().FirstOrDefault(et => et._text == s);

            return Tuple.Create(edmType != null, edmType);
        }

        public static EdmType FromSystemType(Type systemType)
        {
            if (EdmTypeMap.ContainsKey(systemType))
            {
                return EdmTypeMap[systemType];
            }

            return EdmType.String;
        }

        public override string ToString()
        {
            return _text;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            return Equals(obj as EdmType);
        }

        public override int GetHashCode()
        {
            return _text.GetHashCode();
        }

        public bool Equals(EdmType other)
        {
            if (other == null) return false;

            return other._text.Equals(_text);
        }

        private static IEnumerable<EdmType> EnumerateTypes()
        {
            var edmTypes = from field in typeof(EdmType).GetFields(BindingFlags.Public | BindingFlags.Static)
                           where field.FieldType == typeof(EdmType)
                           select ((EdmType)field.GetValue(null));
            return edmTypes;
        }
    }
}
