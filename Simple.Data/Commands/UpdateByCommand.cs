using System;
using System.Dynamic;
using System.Linq;

namespace Simple.Data.Commands
{
    class UpdateByCommand : ICommand
    {
        public bool IsCommandFor(string method)
        {
            return method.StartsWith("UpdateBy") ||
                   method.StartsWith("update_by_", StringComparison.InvariantCultureIgnoreCase);
        }

        public object Execute(Database database, string tableName, InvokeMemberBinder binder, object[] args)
        {
            var criteria = MethodNameParser.ParseFromBinder(binder, args);
            var data = binder.NamedArgumentsToDictionary(args)
                .Where(kvp => !criteria.ContainsKey(kvp.Key))
                .ToDictionary();
            return database.Adapter.Update(tableName, data, criteria);
        }
    }
}
