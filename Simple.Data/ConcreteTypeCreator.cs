using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

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

        public bool TryCreate(HomogenizedKeyDictionary data, out object result)
        {
            bool anyPropertiesSet = false;
            object obj = Activator.CreateInstance(_concreteType);
            foreach (var propertyInfo in _concreteType.GetProperties().Where(pi => CanSetProperty(pi, data)))
            {
                propertyInfo.SetValue(obj, data[propertyInfo.Name], null);
                anyPropertiesSet = true;
            }

            result = anyPropertiesSet ? obj : null;

            return anyPropertiesSet;
        }

        private static bool CanSetProperty(PropertyInfo propertyInfo, HomogenizedKeyDictionary data)
        {
            return data.ContainsKey(propertyInfo.Name) &&
                   !(propertyInfo.PropertyType.IsValueType && data[propertyInfo.Name] == null);
        }
    }
}