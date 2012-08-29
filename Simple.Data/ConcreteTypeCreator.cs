using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Simple.Data.Extensions;
using System.Collections;

namespace Simple.Data
{
    using System.Diagnostics;
    using System.Linq.Expressions;
    using System.Threading;

    internal class ConcreteTypeCreator
    {
        private readonly Lazy<Func<IDictionary<string, object>, object>> _func;
        private static readonly Dictionary<Type, ConcreteTypeCreator> Creators;
        private static readonly ICollection CreatorsCollection;

        static ConcreteTypeCreator()
        {
            CreatorsCollection = Creators = new Dictionary<Type, ConcreteTypeCreator>();
        }

        private ConcreteTypeCreator(Lazy<Func<IDictionary<string, object>, object>> func)
        {
            _func = func;
        }

        public object Create(IDictionary<string, object> source)
        {
            var func = _func.Value;
            return func(source);
        }

        public bool TryCreate(IDictionary<string,object> source, out object result)
        {
            try
            {
                result = Create(source);
                return true;
            }
            catch (Exception)
            {
                result = null;
                return false;
            }
        }

        public static ConcreteTypeCreator Get(Type targetType)
        {
            if (CreatorsCollection.IsSynchronized && Creators.ContainsKey(targetType))
            {
                return Creators[targetType];
            }

            lock (CreatorsCollection.SyncRoot)
            {
                if (Creators.ContainsKey(targetType)) return Creators[targetType];

                var creator = BuildCreator(targetType);
                Creators.Add(targetType, creator);
                return creator;
            }
        }

        private static ConcreteTypeCreator BuildCreator(Type targetType)
        {
            var creator = new ConcreteTypeCreator(new Lazy<Func<IDictionary<string, object>, object>>(() => BuildLambda(targetType), LazyThreadSafetyMode.PublicationOnly));
            return creator;
        }

        private static Func<IDictionary<string, object>, object> BuildLambda(Type targetType)
        {
            var param = Expression.Parameter(typeof (IDictionary<string, object>), "source");
            var obj = Expression.Variable(targetType, "obj");

            var create = CreateNew(targetType, obj);

            var assignments = Expression.Block(
                targetType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Where(PropertyIsConvertible)
                    .Select(p => new PropertySetterBuilder(param, obj, p).CreatePropertySetter()));

            var block = Expression.Block(new[] {obj},
                                         create,
                                         assignments,
                                         obj);

            var lambda = Expression.Lambda<Func<IDictionary<string, object>, object>>(block, param).Compile();
            return lambda;
        }

        private static bool PropertyIsConvertible(PropertyInfo property)
        {
            return property.CanWrite || property.PropertyType.IsGenericCollection();
        }

        private static BinaryExpression CreateNew(Type targetType, ParameterExpression obj)
        {
            var ctor = targetType.GetConstructor(Type.EmptyTypes);
            Debug.Assert(ctor != null);
            var create = Expression.Assign(obj, Expression.New(ctor)); // obj = new T();
            return create;
        }
    }
}