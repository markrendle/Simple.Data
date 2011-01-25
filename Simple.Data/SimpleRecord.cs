using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading;
using Simple.Data.Extensions;

namespace Simple.Data
{
    public partial class SimpleRecord : DynamicObject
    {
        private readonly ConcreteObject _concreteObject = new ConcreteObject();
        private readonly IDictionary<string, object> _data;
        private readonly DataStrategy _database;
        private readonly string _tableName;

        public SimpleRecord()
        {
            _data = new Dictionary<string, object>();
        }

        public SimpleRecord(Database database)
        {
            _data = new Dictionary<string,object>();
            _database = database;
        }

        internal SimpleRecord(IDictionary<string, object> data)
            : this(data, null)
        {
        }

        internal SimpleRecord(IDictionary<string, object> data, string tableName)
            : this(data, tableName, null)
        {
        }

        internal SimpleRecord(IDictionary<string, object> data, string tableName, DataStrategy dataStrategy)
        {
            _tableName = tableName;
            _database = dataStrategy;
            _data = data;
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
                    result = new SimpleRecord(relatedRows.Single(), binder.Name, _database);
                }
                else
                {
                    result = new SimpleResultSet(relatedRows.Select(dict => new SimpleRecord(dict, binder.Name, _database)));
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