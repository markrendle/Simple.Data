using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Shitty.Data.Ado;

namespace Shitty.Data.SqlServer
{
    [Export(typeof(IQueryPager))]
    public class SqlQueryPager : IQueryPager
    {
        private static readonly Regex ColumnExtract = new Regex(@"SELECT\s*(.*)\s*(\sFROM.*)", RegexOptions.Multiline | RegexOptions.IgnoreCase);
        private static readonly Regex SelectMatch = new Regex(@"^SELECT\s*(DISTINCT)?", RegexOptions.IgnoreCase);
        private static readonly Regex LeftJoinMatch = new Regex(@"\sLEFT JOIN .*? ON \(.*?\)", RegexOptions.Multiline | RegexOptions.IgnoreCase);

        public IEnumerable<string> ApplyLimit(string sql, int take)
        {
            yield return SelectMatch.Replace(sql, match => match.Value + " TOP " + take + " ");
        }

        public IEnumerable<string> ApplyPaging(string sql, string[] keys, int skip, int take)
        {
            if (keys == null || keys.Length == 0)
            {
                throw new AdoAdapterException("Cannot apply paging to table with no primary key.");
            }

            sql = sql.Replace(Environment.NewLine, " ");
            var builder = new StringBuilder("WITH __Data AS (SELECT ");

            var match = ColumnExtract.Match(sql);
            var columns = match.Groups[1].Value.Trim();
            var fromEtc = match.Groups[2].Value.Trim();

            builder.Append(string.Join(",", keys));

            var orderBy = ExtractOrderBy(columns, keys, ref fromEtc);

            builder.AppendFormat(", ROW_NUMBER() OVER({0}) AS [_#_]", orderBy);
            builder.AppendLine();
            builder.Append(LeftJoinMatch.Replace(fromEtc, ""));
            builder.AppendLine(")");
            builder.AppendFormat("SELECT {0} FROM __Data ", columns);
            builder.AppendFormat("JOIN {0} ON ",
                                 keys[0].Substring(0, keys[0].LastIndexOf(".", StringComparison.OrdinalIgnoreCase)));
            if (keys.Length > 1)
                builder.AppendFormat(string.Join(" AND ", keys.Select(MakeDataJoin)));
            else
                builder.AppendFormat(MakeDataJoin(keys[0]));
            var groupBy = ExtractGroupBy(ref fromEtc);
            var rest = Regex.Replace(fromEtc, @"^from (\[.*?\]\.\[.*?\])", @"");
            builder.Append(rest);
            
            builder.AppendFormat(" AND [_#_] BETWEEN {0} AND {1}", skip + 1, skip + take);
            if (!string.IsNullOrWhiteSpace(groupBy))
                builder.AppendFormat(" {0}", groupBy);
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

        private static string ExtractGroupBy(ref string fromEtc)
        {
            string groupBy = string.Empty;
            int index = fromEtc.IndexOf("GROUP BY", StringComparison.InvariantCultureIgnoreCase);
            if (index > -1)
            {
                groupBy = fromEtc.Substring(index).Trim();
                fromEtc = fromEtc.Remove(index).Trim();
            }
            return groupBy;
        }
    }
}
