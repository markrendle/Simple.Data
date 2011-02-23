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
    internal class ConcreteTypeCreator
    {
        private static readonly ConcurrentDictionary<Type, ConcreteTypeCreator> Cache =
            new ConcurrentDictionary<Type, ConcreteTypeCreator>();

        private readonly Type _concreteType;

        private ConcreteTypeCreator(Type concreteType)
        {
            _concreteType = concreteType;
        }

        public Type ConcreteType
        {
            get { return _concreteType; }
        }

        public static ConcreteTypeCreator Get(Type concreteType)
        {
            return Cache.GetOrAdd(concreteType, type => new ConcreteTypeCreator(type));
        }

        public bool TryCreate(IDictionary<string, object> data, out object result)
        {
            bool anyPropertiesSet = false;
            object obj = Activator.CreateInstance(_concreteType);
            object value;
            foreach (var propertyInfo in _concreteType.GetProperties().Where(pi => CanSetProperty(pi, data)))
            {
                value = data[propertyInfo.Name.Homogenize()];

                if (ConcreteCollectionTypeCreator.IsCollectionType(propertyInfo.PropertyType))
                {
                    if (!ConcreteCollectionTypeCreator.TryCreate(propertyInfo.PropertyType, (IEnumerable)value, out value))
                        continue;
                }
                else
                {
                    var subData = value as IDictionary<string, object>;
                    if (subData != null && !ConcreteTypeCreator.Get(propertyInfo.PropertyType).TryCreate(subData, out value))
                        continue;
                }

                propertyInfo.SetValue(obj, value, null);
                anyPropertiesSet = true;
            }

            result = anyPropertiesSet ? obj : null;

            return anyPropertiesSet;
        }

        private static bool CanSetProperty(PropertyInfo propertyInfo, IDictionary<string, object> data)
        {
            return data.ContainsKey(propertyInfo.Name) &&
                   !(propertyInfo.PropertyType.IsValueType && data[propertyInfo.Name] == null);
        }
    }
}