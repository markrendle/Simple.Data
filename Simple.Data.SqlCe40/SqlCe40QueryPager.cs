using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Simple.Data.Ado;

namespace Simple.Data.SqlCe40
{
    [Export(typeof(IQueryPager))]
    public class SqlCe40QueryPager : IQueryPager
    {
        private static readonly Regex ColumnExtract = new Regex(@"SELECT\s*(.*)\s*(FROM.*)", RegexOptions.Multiline | RegexOptions.IgnoreCase);
        
        public string ApplyPaging(string sql, string skipParameterName, string takeParameterName)
        {
            if (sql.IndexOf("order by", StringComparison.InvariantCultureIgnoreCase) < 0)
            {
                var match = ColumnExtract.Match(sql);
                var columns = match.Groups[1].Value.Trim();
                sql += " ORDER BY " + columns.Split(',').First().Trim();
            }

            return string.Format("{0} OFFSET {1} ROWS FETCH NEXT {2} ROWS ONLY", sql, skipParameterName, takeParameterName);
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
            }
            return orderBy;
        }
    }
}
