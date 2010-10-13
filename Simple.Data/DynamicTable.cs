using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using Simple.Data.Commands;

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
            var command = CommandFactory.GetCommandFor(binder.Name);
            if (command != null)
            {
                result = command.Execute(_database, _tableName, binder, args);
                return true;
            }

            result = null;
            return false;
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
