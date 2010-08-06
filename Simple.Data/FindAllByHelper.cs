using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace Simple.Data
{
    class FindAllByHelper : FindHelper
    {
        private readonly Database _database;
        private readonly string _tableName;

        public FindAllByHelper(Database database, string tableName)
        {
            _database = database;
            _tableName = tableName;
        }

        public object Run(InvokeMemberBinder binder, IList<object> args)
        {
            string sql = GetFindBySql(_tableName, binder.Name.Substring(9), args);

            return _database.Query(sql, args.ToArray());
        }
    }
}
