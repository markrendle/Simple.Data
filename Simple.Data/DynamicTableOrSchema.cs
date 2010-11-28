using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace Simple.Data
{
    internal class DynamicTableOrSchema : DynamicObject
    {
        private readonly string _name;
        private readonly Database _database;
        private readonly ConcurrentDictionary<string, dynamic> _tablesAndSchema;

        public DynamicTableOrSchema(string name, Database database, ConcurrentDictionary<string, dynamic> tablesAndSchema)
        {
            _name = name;
            _database = database;
            _tablesAndSchema = tablesAndSchema;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            _tablesAndSchema.TryUpdate(_name, new DynamicTable(_name, _database), this);
            return ((DynamicTable) _tablesAndSchema[_name]).TryInvokeMember(binder, args, out result);
        }
    }
}
