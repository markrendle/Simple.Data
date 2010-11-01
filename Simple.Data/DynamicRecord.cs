using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading;
using Simple.Data.Ado;
using Simple.Data.Ado.Schema;
using Simple.Data.Extensions;

namespace Simple.Data
{
    public partial class DynamicRecord : DynamicObject
    {
        private readonly ConcreteObject _concreteObject = new ConcreteObject();
        private readonly HomogenizedKeyDictionary _data;
        private readonly Database _database;
        private readonly string _tableName;

        public DynamicRecord()
        {
            _data = new HomogenizedKeyDictionary();
        }

        public DynamicRecord(Database database)
        {
            _data = new HomogenizedKeyDictionary();
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
            _data = new HomogenizedKeyDictionary(data);
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (_data.ContainsKey(binder.Name))
            {
                result = _data[binder.Name];
                return true;
            }
            if (TryGetJoinResults(binder.Name, out result))
            {
                return true;
            }
            return base.TryGetMember(binder, out result);
        }

        private bool TryGetJoinResults(string name, out object result)
        {
            name = name.Homogenize();
            return name.IsPlural() ? TryGetDetail(name, out result) : (TryGetMaster(name, out result));
        }

        private bool TryGetMaster(string name, out object result)
        {
            var adoAdapter = _database.Adapter as AdoAdapter;
            if (adoAdapter != null)
            {
                TableJoin masterJoin = adoAdapter.GetSchema().FindTable(_tableName).GetMaster(name);
                if (masterJoin != null)
                {
                    result = GetMaster(masterJoin);
                    return true;
                }
            }
            result = null;
            return false;
        }

        private bool TryGetDetail(string name, out object result)
        {
            if (_tableName != null)
            {
                var adoAdapter = _database.Adapter as AdoAdapter;
                if (adoAdapter != null)
                {
                    TableJoin detailJoin = adoAdapter.GetSchema().FindTable(_tableName).GetDetail(name);
                    if (detailJoin != null)
                    {
                        result = GetDetail(detailJoin);
                        return true;
                    }
                }
            }
            result = null;
            return false;
        }

        private dynamic GetMaster(TableJoin masterJoin)
        {
            var criteria = new Dictionary<string, object>
                               {{masterJoin.MasterColumn.ActualName, _data[masterJoin.DetailColumn.HomogenizedName]}};
            IDictionary<string, object> dict =
                _database.Adapter.Find(masterJoin.Master.ActualName,
                                       ExpressionHelper.CriteriaDictionaryToExpression(masterJoin.Master.ActualName,
                                                                                       criteria)).FirstOrDefault();

            return dict != null ? new DynamicRecord(dict, masterJoin.Master.ActualName, _database) : null;
        }

        private IEnumerable<dynamic> GetDetail(TableJoin detailJoin)
        {
            var criteria = new Dictionary<string, object>
                               {{detailJoin.DetailColumn.ActualName, _data[detailJoin.MasterColumn.HomogenizedName]}};
            return _database.Adapter.Find(detailJoin.Detail.ActualName,
                                          ExpressionHelper.CriteriaDictionaryToExpression(detailJoin.Detail.ActualName,
                                                                                          criteria))
                .Select(dict => new DynamicRecord(dict, detailJoin.Detail.ActualName, _database));
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            _data[binder.Name.Homogenize()] = value;
            return true;
        }

        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            result = _concreteObject.Get(binder.Type, _data);
            return result != null;
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return _data.Keys.AsEnumerable();
        }
    }
}