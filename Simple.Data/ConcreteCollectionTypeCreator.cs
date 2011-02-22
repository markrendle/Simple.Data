using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Dynamic;
using System.Linq.Expressions;

namespace Simple.Data
{
    internal static class ConcreteCollectionTypeCreator
    {
        private static readonly List<Creator> _creators = new List<Creator>
        {
            new GenericSetCreator(),
            new GenericListCreator(),
            new NonGenericListCreator()
        };

        public static bool IsCollectionType(Type type)
        {
            return _creators.Any(c => c.IsCollectionType(type));
        }

        public static bool TryCreate(Type type, IEnumerable items, out object result)
        {
            return _creators.First(c => c.IsCollectionType(type)).TryCreate(type, items, out result);
        }

        private abstract class Creator
        {
            public abstract bool IsCollectionType(Type type);

            public abstract bool TryCreate(Type type, IEnumerable items, out object result);

            protected bool TryConvertElement(Type type, object value, out object result)
            {
                result = null;
                if (value == null)
                    return true;

                var valueType = value.GetType();

                if (type.IsAssignableFrom(valueType))
                {
                    result = value;
                    return true;
                }

                try
                {
                    var code = System.Convert.GetTypeCode(value);

                    if (type.IsEnum)
                    {
                        if (value is string)
                        {
                            result = Enum.Parse(type, (string)value);
                            return true;
                        }
                        else
                        {
                            result = Enum.ToObject(type, value);
                            return true;
                        }
                    }
                    else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        result = System.Convert.ChangeType(value, Nullable.GetUnderlyingType(type));
                        return true;
                    }
                    else if (code != TypeCode.Object)
                    {
                        result = System.Convert.ChangeType(value, type);
                        return true;
                    }
                    else
                    {
                        var data = value as IDictionary<string, object>;
                        if (data != null)
                            return ConcreteTypeCreator.Get(type).TryCreate(data, out result);
                    }
                }
                catch (FormatException)
                {
                    return false;
                }
                catch (ArgumentException)
                {
                    return false;
                }

                return true;
            }

            protected bool TryConvertElements(Type type, IEnumerable items, out Array result)
            {
                result = null;
                List<object> list;
                if (items == null)
                    list = new List<object>();
                else
                    list = items.OfType<object>().ToList();

                var array = Array.CreateInstance(type, list.Count);
                object element;
                for(var i = 0; i < array.Length; i++)
                {
                    if(!TryConvertElement(type, list[i], out element))
                        return false;
                    array.SetValue(element, i);
                }

                result = array;
                return true;
            }
        }

        private class NonGenericListCreator : Creator
        {
            public override bool IsCollectionType(Type type)
            {
                if (type == typeof(string))
                    return false;

                return type == typeof(IEnumerable) ||
                    type == typeof(ICollection) ||
                    type == typeof(IList) ||
                    type == typeof(ArrayList);
            }

            public override bool TryCreate(Type type, IEnumerable items, out object result)
            {
                var list = new ArrayList(items.OfType<object>().ToList());
                result = list;
                return true;
            }
        }

        private class GenericListCreator : Creator
        {
            private static readonly Type _openListType = typeof(List<>);

            public override bool IsCollectionType(Type type)
            {
                if (!type.IsGenericType)
                    return false;

                var genericTypeDef = type.GetGenericTypeDefinition();
                if(genericTypeDef.GetGenericArguments().Length != 1)
                    return false;

                return genericTypeDef == typeof(IEnumerable<>) ||
                       genericTypeDef == typeof(ICollection<>) ||
                       genericTypeDef == typeof(IList<>) ||
                       genericTypeDef == typeof(List<>);
            }

            public override bool TryCreate(Type type, IEnumerable items, out object result)
            {
                result = null;
                var elementType = GetElementType(type);
                var listType = _openListType.MakeGenericType(elementType);
                Array elements;
                if (!TryConvertElements(elementType, items, out elements))
                    return false;

                result = Activator.CreateInstance(listType, elements);
                return true;
            }

            private Type GetElementType(Type type)
            {
                return type.GetGenericArguments()[0];
            }
        }

        private class GenericSetCreator : Creator
        {
            private static readonly Type _openSetType = typeof(HashSet<>);

            public override bool IsCollectionType(Type type)
            {
                if (!type.IsGenericType)
                    return false;

                var genericTypeDef = type.GetGenericTypeDefinition();
                if (genericTypeDef.GetGenericArguments().Length != 1)
                    return false;

                return genericTypeDef == typeof(ISet<>) ||
                       genericTypeDef == typeof(HashSet<>);
            }

            public override bool TryCreate(Type type, IEnumerable items, out object result)
            {
                result = null;
                var elementType = GetElementType(type);
                var setType = _openSetType.MakeGenericType(elementType);
                Array elements;
                if (!TryConvertElements(elementType, items, out elements))
                    return false;

                result = Activator.CreateInstance(setType, elements);
                return true;
            }

            private Type GetElementType(Type type)
            {
                return type.GetGenericArguments()[0];
            }
        }
    }
}