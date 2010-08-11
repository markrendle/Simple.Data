using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using Simple.Data.Schema;

namespace Simple.Data
{
    public partial class DynamicRecord : DynamicObject
    {
        private readonly IDictionary<string, object> _data;
        
        public DynamicRecord()
        {
            _data = new Dictionary<string, object>();
        }

        public DynamicRecord(IEnumerable<KeyValuePair<string, object>> data)
        {
            _data = data.Select(kvp => new KeyValuePair<string, object>(kvp.Key.Homogenize(), kvp.Value)).ToDictionary();
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var name = binder.Name.Homogenize();
            if (_data.ContainsKey(name))
            {
                result = _data[name];
                return true;
            }
            return base.TryGetMember(binder, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            _data[binder.Name.Homogenize()] = value;
            return true;
        }

        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            bool anyPropertiesSet = false;
            var obj = Activator.CreateInstance(binder.Type);
            foreach (var propertyInfo in
                binder.Type.GetProperties().Where(propertyInfo => _data.ContainsKey(propertyInfo.Name.Homogenize())))
            {
                propertyInfo.SetValue(obj, _data[propertyInfo.Name.Homogenize()], null);
                anyPropertiesSet = true;
            }

            result = anyPropertiesSet ? obj : null;

            return anyPropertiesSet;
        }
    }
}
