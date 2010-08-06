using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace Simple.Data
{
    public partial class DynamicRecord : DynamicObject
    {
        private readonly IDictionary<string, object> _data;
        
        public DynamicRecord()
        {
            _data = new Dictionary<string, object>();
        }

        public DynamicRecord(IDictionary<string, object> data)
        {
            _data = new Dictionary<string, object>(data);
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (_data.ContainsKey(binder.Name))
            {
                result = _data[binder.Name];
                return true;
            }
            return base.TryGetMember(binder, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            _data[binder.Name] = value;
            return true;
        }

        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            bool anyPropertiesSet = false;
            var obj = Activator.CreateInstance(binder.Type);
            foreach (var propertyInfo in
                binder.Type.GetProperties().Where(propertyInfo => _data.ContainsKey(propertyInfo.Name)))
            {
                propertyInfo.SetValue(obj, _data[propertyInfo.Name], null);
                anyPropertiesSet = true;
            }

            result = anyPropertiesSet ? obj : null;

            return anyPropertiesSet;
        }
    }
}
