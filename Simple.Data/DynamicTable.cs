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
            bool success;

            if (binder.Name.StartsWith("FindBy"))
            {
                result = new FindByHelper(_database, _tableName).Run(binder, args);
                success = true;
            }
            else if (binder.Name.StartsWith("FindAllBy"))
            {
                result = new FindAllByHelper(_database, _tableName).Run(binder, args);
                success = true;
            }
            else if (binder.Name.Equals("Insert", StringComparison.InvariantCultureIgnoreCase))
            {
                result = new InsertHelper(_database, _tableName).Run(binder, args);
                success = true;
            }
            else if (binder.Name.StartsWith("UpdateBy"))
            {
                result = new UpdateHelper(_database, _tableName).Run(binder, args);
                success = true;
            }
            else
            {
                success = base.TryInvokeMember(binder, args, out result);
            }

            return success;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (binder.Name == "All")
            {
                result = GetAll();
                return true;
            }
            return base.TryGetMember(binder, out result);
        }

        public void Insert(dynamic entity)
        {
            var dictionary = entity as IDictionary<string, object>;
            if (dictionary != null)
            {
                _database.Insert(_tableName, dictionary);
            }
        }

        private object GetAll()
        {
            return _database.Query("select * from " + _tableName);
        }
    }
}
