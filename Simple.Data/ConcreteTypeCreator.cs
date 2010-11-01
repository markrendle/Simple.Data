using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Simple.Data.Ado.Schema;

namespace Simple.Data
{
    internal class ConcreteTypeCreator
    {
        private readonly Type _concreteType;

        public ConcreteTypeCreator(Type concreteType)
        {
            _concreteType = concreteType;
        }

        public Type ConcreteType
        {
            get { return _concreteType; }
        }

        public bool TryCreate(IDictionary<string, object> data, out object result)
        {
            bool anyPropertiesSet = false;
            var obj = Activator.CreateInstance(_concreteType);
            foreach (var propertyInfo in _concreteType.GetProperties().Where(pi => CanSetProperty(pi, data)))
            {
                propertyInfo.SetValue(obj, data[propertyInfo.Name.Homogenize()], null);
                anyPropertiesSet = true;
            }

            result = anyPropertiesSet ? obj : null;

            return anyPropertiesSet;
        }
        private bool CanSetProperty(PropertyInfo propertyInfo, IDictionary<string,object> data)
        {
            return data.ContainsKey(propertyInfo.Name.Homogenize()) && !(propertyInfo.PropertyType.IsValueType && data[propertyInfo.Name.Homogenize()] == null);
        }
    }
}
