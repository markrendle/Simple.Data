using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.Extensions
{
    using System.Collections.Concurrent;
    using System.Linq.Expressions;
    using System.Reflection;

    public static class ObjectEx
    {
        private static readonly ConcurrentDictionary<Type, Func<object, IDictionary<string, object>>> Converters =
            new ConcurrentDictionary<Type, Func<object, IDictionary<string, object>>>();

        public static IDictionary<string, object> ObjectToDictionary(this object obj)
        {
            if (obj == null) return new Dictionary<string, object>();

            return Converters.GetOrAdd(obj.GetType(), MakeToDictionaryFunc)(obj);
        }

        private static Func<object, IDictionary<string, object>> MakeToDictionaryFunc(Type type)
        {
            var param = Expression.Parameter(typeof(object));
            var typed = Expression.Variable(type);
            var newDict = Expression.New(typeof(Dictionary<string, object>));
            var listInit = Expression.ListInit(newDict, GetElementInitsForType(type, typed));

            var block = Expression.Block(new[] { typed },
                                         Expression.Assign(typed, Expression.Convert(param, type)),
                                         listInit);

            return Expression.Lambda<Func<object, IDictionary<String, object>>>(block, param).Compile();
        }

        static readonly MethodInfo DictionaryAddMethod = typeof(Dictionary<string, object>).GetMethod("Add");

        static IEnumerable<ElementInit> GetElementInitsForType(Type type, Expression param)
        {
            return type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.CanRead)
                .Select(p => PropertyToElementInit(p, param));
        }

        static ElementInit PropertyToElementInit(PropertyInfo propertyInfo, Expression instance)
        {
            return Expression.ElementInit(DictionaryAddMethod,
                Expression.Constant(propertyInfo.Name),
                Expression.Convert(Expression.Property(instance, propertyInfo), typeof(object)));
        }

        internal static bool IsAnonymous(this object obj)
        {
            if (obj == null) return false;
            return obj.GetType().Namespace == null;
        }
    }
}
