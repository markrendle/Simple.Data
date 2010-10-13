using System;
using System.Dynamic;

namespace Simple.Data.Commands
{
    class FindByCommand : ICommand
    {
        public bool IsCommandFor(string method)
        {
            return method.StartsWith("FindBy") || method.StartsWith("find_by_", StringComparison.InvariantCultureIgnoreCase);
        }

        public object Execute(Database database, string tableName, InvokeMemberBinder binder, object[] args)
        {
            var criteria = MethodNameParser.ParseFromBinder(binder, args);
            var data = database.Adapter.Find(tableName, criteria);
            return data != null ? new DynamicRecord(data, tableName, database) : null;
        }
    }
}
