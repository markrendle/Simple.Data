using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Simple.Data
{
    public static class DataReaderExtensions
    {
        public static IEnumerable<dynamic> ToDynamicList(this IDataReader reader)
        {
            var list = new List<dynamic>();
            using (reader)
            {
                while (reader.Read())
                {
                    list.Add(reader.ToDynamicRow());
                }
            }

            return list.AsEnumerable();
        }
    }
}
