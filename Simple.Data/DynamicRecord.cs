using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using Simple.Data.Schema;
using System.Data;

namespace Simple.Data
{
    public partial class DynamicRecord : DynamicObject
    {
        private readonly Database _database;
        private readonly string _tableName;
        private readonly IDictionary<string, object> _data;

        public DynamicRecord()
        {
            _data = new Dictionary<string, object>();
        }

        public DynamicRecord(Database database)
        {
            _data = new Dictionary<string, object>();
            _database = database;
        }

        internal DynamicRecord(IEnumerable<KeyValuePair<string, object>> data) : this(data, null)
        {
        }

        internal DynamicRecord(IEnumerable<KeyValuePair<string, object>> data, string tableName)
            : this(data, tableName, null)
        {
        }

        internal DynamicRecord(IEnumerable<KeyValuePair<string, object>> data, string tableName, Database database)
        {
            _tableName = tableName;
            _database = database;
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
            else if (TryGetJoinResults(name, out result))
            {
                return true;
            }
            return base.TryGetMember(binder, out result);
        }

        private bool TryGetJoinResults(string name, out object result)
        {
            return name.IsPlural() ? TryGetDetail(name, out result) :(TryGetMaster(name, out result));
        }

        private bool TryGetMaster(string name, out object result)
        {
            var masterJoin = _database.GetSchema().FindTable(_tableName).GetMaster(name);
            if (masterJoin != null)
            {
                result = GetMaster(masterJoin);
                return true;
            }
            result = null;
            return false;
        }

        private bool TryGetDetail(string name, out object result)
        {
            if (_tableName != null)
            {
                var detailJoin = _database.GetSchema().FindTable(_tableName).GetDetail(name);
                if (detailJoin != null)
                {
                    result = GetDetail(detailJoin);
                    return true;
                }
            }
            result = null;
            return false;
        }

        private dynamic GetMaster(TableJoin masterJoin)
        {
            var criteria = new Dictionary<string, object>
                               {{masterJoin.MasterColumn.ActualName, _data[masterJoin.DetailColumn.HomogenizedName]}};
            var dict = _database.Adapter.Find(masterJoin.Master.ActualName, criteria);

            return dict != null ? new DynamicRecord(dict, masterJoin.Master.ActualName, _database) : null;
        }

        private IEnumerable<dynamic> GetDetail(TableJoin detailJoin)
        {
            var criteria = new Dictionary<string, object> { { detailJoin.DetailColumn.ActualName, _data[detailJoin.MasterColumn.HomogenizedName] } };
            return _database.Adapter.FindAll(detailJoin.Detail.ActualName, criteria)
                .Select(dict => new DynamicRecord(dict, detailJoin.Detail.ActualName, _database));
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
