using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace Simple.Data
{
    class DeleteHelper
    {
        private readonly Database _database;
        private readonly string _tableName;

        public DeleteHelper(Database database, string tableName)
        {
            _database = database;
            _tableName = tableName;
        }

        public object Run(InvokeMemberBinder binder, IList<object> args)
        {
            var delete = BinderHelper.NamedArgumentsToDictionary(binder, args);

            _database.Delete(_tableName, delete);

            return null;
        }
    }
}
