using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace Simple.Data
{
    class UpdateHelper
    {
        private readonly Database _database;
        private readonly string _tableName;

        public UpdateHelper(Database database, string tableName)
        {
            _database = database;
            _tableName = tableName;
        }

        public object Run(InvokeMemberBinder binder, object[] args)
        {
            var byColumns = GetUpdateByColumns(args, binder.Name.Substring(8));
            if (byColumns == null) return 0;

            var setDict = BinderHelper.NamedArgumentsToDictionary(binder, args);
            var criteriaDict = new Dictionary<string, object>();

            foreach (var byColumn in byColumns)
            {
                criteriaDict.Add(byColumn, setDict[byColumn]);
                setDict.Remove(byColumn);
            }

            _database.Update(_tableName, setDict, criteriaDict);
            return null;
        }

        internal static string[] GetUpdateByColumns(IList<object> args, string methodName)
        {
            if (args == null) throw new ArgumentNullException("args");
            if (args.Count == 0) throw new ArgumentException("No parameters specified.");

            var columns = methodName.Split(new[] { "And" }, StringSplitOptions.RemoveEmptyEntries);

            if (columns.Length == 0) throw new ArgumentException("No columns specified.");
            if (args.Count < columns.Length) throw new ArgumentException("Not enough update columns specified.");
            if (args.Count == columns.Length) return null; // No values to actually update. Fail silently.
            return columns;
        }
    }
}
