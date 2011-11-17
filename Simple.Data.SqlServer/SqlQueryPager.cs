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

        public string ApplyPaging(string sql, string skipParameterName, string takeParameterName)
        {
            var builder = new StringBuilder("WITH __Data AS (SELECT ");

            var match = ColumnExtract.Match(sql);
            var columns = match.Groups[1].Value.Trim();
            var fromEtc = match.Groups[2].Value.Trim();

            builder.Append(columns);

            var orderBy = ExtractOrderBy(columns, ref fromEtc);

            builder.AppendFormat(", ROW_NUMBER() OVER({0}) AS [_#_]", orderBy);
            builder.AppendLine();
            builder.Append(fromEtc);
            builder.AppendLine(")");
            builder.AppendFormat("SELECT {0} FROM __Data WHERE [_#_] BETWEEN {1} + 1 AND {1} + {2}", DequalifyColumns(columns),
                                 skipParameterName, takeParameterName);

            return builder.ToString();
        }

        private static string DequalifyColumns(string original)
        {
            var q = from part in original.Split(',')
                    select part.Substring(Math.Max(part.LastIndexOf('.') + 1, part.LastIndexOf('[')));
            return string.Join(",", q);
        }

        private static string ExtractOrderBy(string columns, ref string fromEtc)
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
                orderBy = "ORDER BY " + columns.Split(',').First().Trim();

                var aliasIndex = orderBy.IndexOf(" AS [", StringComparison.InvariantCultureIgnoreCase);

                if (aliasIndex > -1)
                {
                    orderBy = orderBy.Substring(0, aliasIndex);
                }
            }
            return orderBy;
        }
    }
}
