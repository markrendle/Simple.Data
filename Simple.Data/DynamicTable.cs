using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.Data;
using System.Diagnostics;

namespace Simple.Data
{
    public class DynamicTable : DynamicObject
    {
        private readonly string _tableName;
        private readonly Database _database;

        public DynamicTable(string tableName, Database database)
        {
            _tableName = tableName;
            _database = database;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            bool success;

            if (binder.Name.StartsWith("FindBy"))
            {
                result = FindBy(binder, args);
                success = true;
            }
            else if (binder.Name.StartsWith("FindAllBy"))
            {
                result = FindAllBy(binder, args);
                success = true;
            }
            else if (binder.Name.Equals("Insert", StringComparison.InvariantCultureIgnoreCase))
            {
                result = Insert(binder, args);
                success = true;
            }
            else if (binder.Name.StartsWith("UpdateBy"))
            {
                result = Update(binder, args);
                success = true;
            }
            else
            {
                success = base.TryInvokeMember(binder, args, out result);
            }

            return success;
        }

        private object Update(InvokeMemberBinder binder, object[] args)
        {
            var byColumns = GetUpdateByColumns(args, binder.Name.Substring(8));
            if (byColumns == null) return 0;

            var dict = NamedArgumentsToDictionary(binder, args);

            List<object> values = new List<object>();
            List<string> sets = new List<string>();

            foreach (var pair in dict.Where(p => !byColumns.Contains(p.Key)))
            {
                sets.Add(pair.Key + " = ?");
                values.Add(pair.Value);
            }

            var builder = new StringBuilder("update " + _tableName + " set " + string.Join(", ", sets));
            builder.Append(" where " + string.Join(" and ", byColumns.Select(col => col + " = ?")));

            values.AddRange(byColumns.Select(byColumn => dict[byColumn]));

            _database.Execute(builder.ToString(), values.ToArray());
            return null;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (binder.Name == "All")
            {
                result = GetAll();
                return true;
            }
            return base.TryGetMember(binder, out result);
        }

        public void Insert(dynamic entity)
        {
            var dictionary = entity as IDictionary<string, object>;
            if (dictionary != null)
            {
                _database.Insert(_tableName, dictionary);
            }
        }

        private object Insert(InvokeMemberBinder binder, IList<object> args)
        {
            var insert = NamedArgumentsToDictionary(binder, args);

            _database.Insert(_tableName, insert);

            return null;
        }

        private object FindBy(InvokeMemberBinder binder, IList<object> args)
        {
            string sql = GetFindBySql(_tableName, binder.Name.Substring(6), args);

            return _database.Query(sql, args.ToArray()).FirstOrDefault();
        }

        private object FindAllBy(InvokeMemberBinder binder, IList<object> args)
        {
            string sql = GetFindBySql(_tableName, binder.Name.Substring(9), args);

            return _database.Query(sql, args.ToArray());
        }

        private object GetAll()
        {
            return _database.Query(GetAllSql(_tableName));
        }

        internal static string GetAllSql(string tableName)
        {
            return "select * from " + tableName;
        }

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

            var columns = methodName.Split(new[] {"And"}, StringSplitOptions.RemoveEmptyEntries);

            if (columns.Length == 0) throw new ArgumentException("No columns specified.");
            if (columns.Length != args.Count) throw new ArgumentException("Parameter count mismatch.");
            return columns;
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

        internal static Dictionary<string, object> NamedArgumentsToDictionary(InvokeMemberBinder binder, IList<object> args)
        {
            var dict = new Dictionary<string, object>();

            for (int i = 0; i < args.Count; i++)
            {
                dict.Add(binder.CallInfo.ArgumentNames[i], args[i]);
            }

            return dict;
        }
    }
}
