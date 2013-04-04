using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Simple.Data.Ado;

namespace Simple.Data.SqlServer
{
    [Export(typeof(IQueryPager))]
    public class SqlQueryPager : IQueryPager
    {
        private static readonly Regex ColumnExtract = new Regex(@"SELECT\s*(.*)\s*(FROM.*)", RegexOptions.Multiline | RegexOptions.IgnoreCase);
        private static readonly Regex SelectMatch = new Regex(@"^SELECT\s*(DISTINCT)?", RegexOptions.IgnoreCase);

        public IEnumerable<string> ApplyLimit(string sql, int take)
        {
            yield return SelectMatch.Replace(sql, match => match.Value + " TOP " + take + " ");
        }

        public IEnumerable<string> ApplyPaging(string sql, string[] keys, int skip, int take)
        {
            var builder = new StringBuilder("WITH __Data AS (SELECT ");

            var match = ColumnExtract.Match(sql);
            var columns = match.Groups[1].Value.Trim();
            var fromEtc = match.Groups[2].Value.Trim();

            builder.Append(string.Join(",", keys));

            var orderBy = ExtractOrderBy(columns, keys, ref fromEtc);

            builder.AppendFormat(", ROW_NUMBER() OVER({0}) AS [_#_]", orderBy);
            builder.AppendLine();
            builder.Append(fromEtc);
            builder.AppendLine(")");
            builder.AppendFormat("SELECT {0} FROM __Data ", columns);
            builder.AppendFormat("JOIN {0} ON ",
                                 keys[0].Substring(0, keys[0].LastIndexOf(".", StringComparison.OrdinalIgnoreCase)));
            if (keys.Length > 1)
                builder.AppendFormat(string.Join(" AND ", keys.Select(MakeDataJoin)));
            else
                builder.AppendFormat(string.Join(" ", keys.Select(MakeDataJoin)));
            var rest = Regex.Replace(fromEtc, @"^from (\[.*?\]\.\[.*?\])", @"");
            builder.Append(rest);
            
            builder.AppendFormat(" AND [_#_] BETWEEN {0} AND {1}", skip + 1, skip + take);

            yield return builder.ToString();
        }

        private static string MakeDataJoin(string key)
        {
            return key + " = __Data" + key.Substring(key.LastIndexOf(".", StringComparison.OrdinalIgnoreCase));
        }

        private static string DequalifyColumns(string original)
        {
            var q = from part in original.Split(',')
                    select part.Substring(Math.Max(part.LastIndexOf('.') + 1, part.LastIndexOf('[')));
            return string.Join(",", q);
        }

        private static string ExtractOrderBy(string columns, string[] keys, ref string fromEtc)
        {
            string orderBy;
            int index = fromEtc.IndexOf("ORDER BY", StringComparison.InvariantCultureIgnoreCase);
            if (index > -1)
            {
                orderBy = fromEtc.Substring(index).Trim();
                fromEtc = fromEtc.Remove(index).Trim();
            }
            else
            {
                orderBy = "ORDER BY " + string.Join(", ", keys);
            }
            return orderBy;
        }
    }
}
