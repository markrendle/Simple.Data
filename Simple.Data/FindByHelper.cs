using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace Simple.Data
{
    class FindByHelper : FindHelper
    {
        private readonly Database _database;
        private readonly string _tableName;

        public FindByHelper(Database database, string tableName)
        {
            _database = database;
            _tableName = tableName;
        }

        public object Run(InvokeMemberBinder binder, IList<object> args)
        {
            string sql = GetFindBySql(_tableName, binder.Name.Substring(6), args);

            return _database.QueryTable(_tableName, sql, args.ToArray()).FirstOrDefault();
        }
    }
}
