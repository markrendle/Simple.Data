using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using Simple.Data.Extensions;

namespace Simple.Data.Commands
{
    class InsertCommand : ICommand
    {
        public bool IsCommandFor(string method)
        {
            return method.Equals("insert", StringComparison.InvariantCultureIgnoreCase);
        }

        public object Execute(Database database, string tableName, InvokeMemberBinder binder, object[] args)
        {
            return DoInsert(binder, args, database, tableName).ToDynamicRecord(tableName, database);
        }

        private static IDictionary<string, object> DoInsert(InvokeMemberBinder binder, object[] args, Database database, string tableName)
        {
            return binder.HasSingleUnnamedArgument()
                ?
                InsertEntity(args[0], database, tableName)
                :
                InsertDictionary(binder, args, database, tableName);
        }

        private static IDictionary<string, object> InsertDictionary(InvokeMemberBinder binder, object[] args, Database database, string tableName)
        {
            return database.Adapter.Insert(tableName, binder.NamedArgumentsToDictionary(args));
        }

        private static IDictionary<string,object> InsertEntity(object entity, Database database, string tableName)
        {
            var dictionary = entity as IDictionary<string, object>;
            if (dictionary == null)
                throw new SimpleDataException("Could not discover data in object.");
            return database.Adapter.Insert(tableName, dictionary);
        }
    }
}
