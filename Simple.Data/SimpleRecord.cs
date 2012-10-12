using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading;
using Simple.Data.Extensions;

namespace Simple.Data
{
    public partial class SimpleRecord : DynamicObject, ICloneable
    {
        private static readonly DictionaryCloner Cloner = new DictionaryCloner();
        private readonly ConcreteObject _concreteObject = new ConcreteObject();
        private readonly IDictionary<string, object> _data;
        private readonly DataStrategy _database;
        private readonly string _tableName;

        public SimpleRecord()
        {
            _data = new Dictionary<string, object>(HomogenizedEqualityComparer.DefaultInstance);
        }

        public SimpleRecord(DataStrategy database)
        {
            _data = new Dictionary<string, object>(HomogenizedEqualityComparer.DefaultInstance);
            _database = database;
        }

        public SimpleRecord(IDictionary<string, object> data)
            : this(data, null)
        {
        }

        public SimpleRecord(IDictionary<string, object> data, string tableName)
            : this(data, tableName, null)
        {
        }

        public SimpleRecord(IDictionary<string, object> data, string tableName, DataStrategy dataStrategy)
        {
            _tableName = tableName;
            _database = dataStrategy;
            _data = data ?? new Dictionary<string, object>();
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (_data.ContainsKey(binder.Name))
            {
                result = _data[binder.Name];
                var converted = ConvertResult(result);
                if (!ReferenceEquals(result, converted))
                    _data[binder.Name] = result = converted;

                return true;
            }

            if (_tableName == null)
            {
                result = null;
                return false;
            }

            if (_database != null)
            {
                try
                {
                    var relatedAdapter = _database.GetAdapter() as IAdapterWithRelation;
                    if (relatedAdapter != null && relatedAdapter.IsValidRelation(_tableName, binder.Name))
                    {
                        result = GetRelatedData(binder, relatedAdapter);
                        return true;
                    }
                }
                catch (UnresolvableObjectException)
                {
                    throw new UnresolvableObjectException("Column not found.");
                }
            }
            return base.TryGetMember(binder, out result);
        }

        private object GetRelatedData(GetMemberBinder binder, IAdapterWithRelation relatedAdapter)
        {
            object result;
            var related = relatedAdapter.FindRelated(_tableName, _data, binder.Name);
            var query = related as SimpleQuery;
            if (query != null)
            {
                query.SetDataStrategy(_database);
                result = query;
            }
            else
            {
                result = related is IDictionary<string, object>
                             ? (object) new SimpleRecord(related as IDictionary<string, object>, binder.Name, _database)
                             : ((IEnumerable<IDictionary<string, object>>) related).Select(
                                       dict => new SimpleRecord(dict, binder.Name, _database)).ToList<dynamic>();
                _data[binder.Name] = result;

            }
            return result;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            _data[binder.Name] = value;
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

        private object ConvertResult(object result)
        {
            if (result is SimpleList || result is SimpleRecord) return result;

            var subRecord = result as IDictionary<string, object>;
            if (subRecord != null)
                return new SimpleRecord(subRecord);

            var list = result as IEnumerable<object>;
            if (list != null)
            {
                return new SimpleList(list.Select(ConvertResult));
            }

            var func = result as Func<IDictionary<string, object>, object>;
            if (func != null)
            {
                result = func(_data);
            }

            return result;
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public object Clone()
        {
            return new SimpleRecord(Cloner.CloneDictionary(_data), _tableName, _database);
        }

        public object ToScalar()
        {
            if (_data == null || _data.Count == 0) return null;
            return _data.First().Value;
        }

        public T ToScalar<T>()
        {
            if (_data == null || _data.Count == 0) return default(T);
            return (T)_data.First().Value;
        }
    }
}