using System;
using System.Collections.Generic;
using System.Text;

namespace Simple.Data.Ado
{
    static class FindHelper
    {
        internal static string GetFindBySql(string tableName, IDictionary<string, object> criteria)
        {
            var sqlBuilder = new StringBuilder("select * from " + tableName);
            var keyword = "where";

            foreach (var criterion in criteria)
            {
                sqlBuilder.AppendFormat(" {0} {1} = ?", keyword, criterion.Key);
                if (keyword == "where")
                {
                    keyword = "and";
                }
            }

            return sqlBuilder.ToString();
        }
    }
}
