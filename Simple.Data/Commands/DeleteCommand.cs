using System;
using System.Dynamic;

namespace Simple.Data.Commands
{
    class DeleteCommand : ICommand
    {
        public bool IsCommandFor(string method)
        {
            return method.Equals("delete", StringComparison.InvariantCultureIgnoreCase);
        }

        public object Execute(Database database, string tableName, InvokeMemberBinder binder, object[] args)
        {
            return database.Adapter.Delete(tableName, binder.NamedArgumentsToDictionary(args));
        }
    }
}
