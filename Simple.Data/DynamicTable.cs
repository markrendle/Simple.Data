using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.Data;
using System.Diagnostics;

namespace Simple.Data
{
    public class DynamicTable : DynamicObject
    {
        private readonly string _tableName;
        private readonly Database _database;

        public DynamicTable(string tableName, Database database)
        {
            _tableName = tableName;
            _database = database;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            result = null;
            bool success = false;

            if (binder.Name.Contains("By"))
            {
                var criteria = MethodNameParser.ParseFromBinder(binder, args);
                if (binder.Name.StartsWith("FindBy"))
                {
                    var data = _database.Adapter.Find(_tableName, criteria);
                    result = data != null ? new DynamicRecord(data, _tableName, _database) : null;
                    success = true;
                }
                else if (binder.Name.StartsWith("FindAllBy"))
                {
                    var data = _database.Adapter.FindAll(_tableName, criteria);
                    result = data != null
                                 ? data.Select(dict => new DynamicRecord(dict, _tableName, _database))
                                 : Enumerable.Empty<dynamic>();
                    success = true;
                }
                else if (binder.Name.StartsWith("UpdateBy"))
                {
                    var data = binder.NamedArgumentsToDictionary(args)
                        .Where(kvp => !criteria.ContainsKey(kvp.Key))
                        .ToDictionary();
                    result = _database.Adapter.Update(_tableName, data, criteria);
                    success = true;
                }
            }
            else
            {
                if (binder.Name.Equals("Insert", StringComparison.InvariantCultureIgnoreCase))
                {
                    result = _database.Adapter.Insert(_tableName, binder.NamedArgumentsToDictionary(args));
                    success = true;
                }
                else if (binder.Name.Equals("Delete", StringComparison.InvariantCultureIgnoreCase))
                {
                    result = _database.Adapter.Delete(_tableName, binder.NamedArgumentsToDictionary(args));
                    success = true;
                }
                else
                {
                    success = base.TryInvokeMember(binder, args, out result);
                }
            }

            return success;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (binder.Name == "All")
            {
                result = GetAll().ToList();
                return true;
            }
            return base.TryGetMember(binder, out result);
        }

        public void Insert(dynamic entity)
        {
            var dictionary = entity as IDictionary<string, object>;
            if (dictionary != null)
            {
                _database.Adapter.Insert(_tableName, dictionary);
            }
        }

        private IEnumerable<dynamic> GetAll()
        {
            return _database.Adapter.FindAll(_tableName).Select(dict => new DynamicRecord(dict, _tableName, _database));
        }
    }
}
