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
            var relatedAdapter = _database.Adapter as IAdapterWithRelation;
            if (relatedAdapter != null && relatedAdapter.IsValidRelation(_tableName, binder.Name))
            {
                var relatedRows = relatedAdapter.FindRelated(_tableName, _data, binder.Name);
                if (relatedRows.Count() == 1 && !binder.Name.IsPlural())
                {
                    result = new DynamicRecord(relatedRows.Single(), binder.Name, _database);
                }
                else
                {
                    result = relatedRows.Select(dict => new DynamicRecord(dict, binder.Name, _database));
                }
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
            result = _concreteObject.Get(binder.Type, _data);
            return result != null;
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return _data.Keys.AsEnumerable();
        }
    }
}