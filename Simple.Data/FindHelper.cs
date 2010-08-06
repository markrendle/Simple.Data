using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace Simple.Data
{
    abstract class FindHelper
    {
        internal static string GetFindBySql(string tableName, string methodName, IList<object> args)
        {
            string[] columns = GetFindByColumns(args, methodName);

            var sqlBuilder = new StringBuilder("select * from " + tableName);
            sqlBuilder.AppendFormat(" where {0} = ?", columns[0]);

            for (int i = 1; i < columns.Length; i++)
            {
                sqlBuilder.AppendFormat(" and {0} = ?", columns[i]);
            }

            return sqlBuilder.ToString();
        }

        internal static string[] GetFindByColumns(IList<object> args, string methodName)
        {
            if (args == null) throw new ArgumentNullException("args");
            if (args.Count == 0) throw new ArgumentException("No parameters specified.");

            var columns = methodName.Split(new[] { "And" }, StringSplitOptions.RemoveEmptyEntries);

            if (columns.Length == 0) throw new ArgumentException("No columns specified.");
            if (columns.Length != args.Count) throw new ArgumentException("Parameter count mismatch.");
            return columns;
        }
    }
}
