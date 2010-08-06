using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace Simple.Data
{
    class InsertHelper
    {
        private readonly Database _database;
        private readonly string _tableName;

        public InsertHelper(Database database, string tableName)
        {
            _database = database;
            _tableName = tableName;
        }

        public object Run(InvokeMemberBinder binder, IList<object> args)
        {
            var insert = BinderHelper.NamedArgumentsToDictionary(binder, args);

            _database.Insert(_tableName, insert);

            return null;
        }
    }
}
