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

        public object Execute(DataStrategy dataStrategy, DynamicTable table, InvokeMemberBinder binder, object[] args)
        {
            return DoInsert(binder, args, dataStrategy, table.GetQualifiedName()).ToDynamicRecord(table.GetQualifiedName(), dataStrategy);
        }

        private static IDictionary<string, object> DoInsert(InvokeMemberBinder binder, object[] args, DataStrategy dataStrategy, string tableName)
        {
            return binder.HasSingleUnnamedArgument()
                ?
                InsertEntity(args[0], dataStrategy, tableName)
                :
                InsertDictionary(binder, args, dataStrategy, tableName);
        }

        private static IDictionary<string, object> InsertDictionary(InvokeMemberBinder binder, object[] args, DataStrategy dataStrategy, string tableName)
        {
            return dataStrategy.Insert(tableName, binder.NamedArgumentsToDictionary(args));
        }

        private static IDictionary<string,object> InsertEntity(object entity, DataStrategy dataStrategy, string tableName)
        {
            var dictionary = entity as IDictionary<string, object>;
            if (dictionary == null)
            {
                dictionary = ObjectEx.ObjectToDictionary(entity);
                if (dictionary.Count == 0)
                    throw new SimpleDataException("Could not discover data in object.");
            }
            return dataStrategy.Insert(tableName, dictionary);
        }
    }
}
