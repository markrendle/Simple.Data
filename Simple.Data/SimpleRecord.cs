using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading;
using Shitty.Data.Extensions;

namespace Shitty.Data
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
            if (TryGetMember(binder.Name, out result) == false)
            {
                return base.TryGetMember(binder, out result);
            }

            return true;
        }

        private bool TryGetMember(string name, out object result)
        {
            if (_data.ContainsKey(name))
            {
                result = _data[name];
                var converted = ConvertResult(result);
                if (!ReferenceEquals(result, converted))
                    _data[name] = result = converted;

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
                    if (relatedAdapter != null && relatedAdapter.IsValidRelation(_tableName, name))
                    {
                        result = GetRelatedData(name, relatedAdapter);
                        return true;
                    }
                }
                catch (UnresolvableObjectException e)
                {
                    throw new UnresolvableObjectException(e.ObjectName, string.Format("Column '{0}' not found.", e.ObjectName), e);
                }
            }

            result = null;
            return false;
        }

        private object GetRelatedData(string name, IAdapterWithRelation relatedAdapter)
        {
            object result;
            var related = relatedAdapter.FindRelated(_tableName, _data, name);
            var query = related as SimpleQuery;
            if (query != null)
            {
                query.SetDataStrategy(_database);
                result = query;
            }
            else
            {
                result = related is IDictionary<string, object>
                             ? (object) new SimpleRecord(related as IDictionary<string, object>, name, _database)
                             : ((IEnumerable<IDictionary<string, object>>) related).Select(
                                       dict => new SimpleRecord(dict, name, _database)).ToList<dynamic>();
                _data[name] = result;

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

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            if (indexes.Length == 1 && indexes[0] is string)
            {
                if (TryGetMember((string) indexes[0], out result))
                {
                    return true;
                }
            }

            return base.TryGetIndex(binder, indexes, out result);
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