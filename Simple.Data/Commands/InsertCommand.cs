using System;
using System.Dynamic;

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
            return database.Adapter.Insert(tableName, binder.NamedArgumentsToDictionary(args));
        }
    }
}
