using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;

namespace Shitty.Data
{
    using System.Text;

    internal sealed class FunctionSignature : IEquatable<FunctionSignature>
    {
        private readonly string _name;
        private readonly IList<Parameter> _parameters;
        private static readonly object obj = new object();

        private FunctionSignature(string name, IList<Parameter> parameters)
        {
            _name = name;
            _parameters = parameters;
        }

        public static string FromBinder(InvokeMemberBinder binder, object[] args)
        {
            if (args.Length == 1 && !(args[0] is IList))
                return string.Format("{0}({1})", binder.Name, (args[0] ?? obj).GetType());

            var builder = new StringBuilder(binder.Name);
            builder.Append("(");
            for (int i = 0; i < args.Length; i++)
            {
                if (binder.CallInfo.ArgumentNames.Count > i && !string.IsNullOrWhiteSpace(binder.CallInfo.ArgumentNames[i]))
                {
                    builder.Append(binder.CallInfo.ArgumentNames[i]);
                    builder.Append(":");
                }
                var type = (args[i] ?? obj).GetType();
                builder.Append(type.FullName);
                var list = args[i] as IList;
                if (list != null)
                {
                    builder.AppendFormat("[{0}]", list.Count);
                }
            }
            builder.Append(")");
            return builder.ToString();
        }

        private static IList<Parameter> GetParameters(InvokeMemberBinder binder, object[] args)
        {
            var parameters = new Parameter[args.Length];
            for (int i = 0; i < args.Length; i++)
            {
                var list = args[i] as IList;
                if (list != null)
                {
                    parameters[i] = new Parameter(NullSafeGetType(args[i]), binder.CallInfo.ArgumentNames.GetOrDefault(i), list.Count);
                }
                else
                {
                    parameters[i] = new Parameter(NullSafeGetType(args[i]), binder.CallInfo.ArgumentNames.GetOrDefault(i));
                }
            }
            return parameters;
        }

        private static Type NullSafeGetType(object o)
        {
            return o == null ? typeof (object) : o.GetType();
        }

        class Parameter : IEquatable<Parameter>
        {
            private readonly Type _type;
            private readonly string _name;
            private readonly int _size;

            public Parameter(Type type) : this(type, null)
            {
            }

            public Parameter(Type type, string name) : this(type, name, 0)
            {
            }

            public Parameter(Type type, int size) : this(type, null, size)
            {
            }

            public Parameter(Type type, string name, int size)
            {
                _type = type;
                _name = name;
                _size = size;
            }

            public bool Equals(Parameter other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return other._type == _type && Equals(other._name, _name) && other._size == _size;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != typeof (Parameter)) return false;
                return Equals((Parameter) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int result = _type.GetHashCode();
                    result = (result*397) ^ (_name != null ? _name.GetHashCode() : 0);
                    result = (result*397) ^ _size;
                    return result;
                }
            }

            public static bool operator ==(Parameter left, Parameter right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(Parameter left, Parameter right)
            {
                return !Equals(left, right);
            }
        }

        public bool Equals(FunctionSignature other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._name, _name) && ParameterListsMatch(other._parameters, _parameters);
        }

        private static bool ParameterListsMatch(IList<Parameter> left, IList<Parameter> right)
        {
            if (left.Count != right.Count) return false;

            for (int i = 0; i < left.Count; i++)
            {
                if (!left[i].Equals(right[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (FunctionSignature)) return false;
            return Equals((FunctionSignature) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = _name.GetHashCode()*397;
                for (int i = 0; i < _parameters.Count; i++)
                {
                    hashCode ^= _parameters[i].GetHashCode();
                }
                return hashCode;
            }
        }

        public static bool operator ==(FunctionSignature left, FunctionSignature right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(FunctionSignature left, FunctionSignature right)
        {
            return !Equals(left, right);
        }
    }

    static class ListEx
    {
        public static T GetOrDefault<T>(this IList<T> list, int index)
        {
            return index < list.Count ? list[index] : default(T);
        }
    }
}