using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Data.Ado.Sql
{
    public class Select
    {
        public string[] KeyColumns { get; set; }
        public string[] OtherColumns { get; set; }
        public string Table { get; set; }
        public string Joins { get; set; }
        public string Where { get; set; }
        public string OrderBy { get; set; }
        public string GroupBy { get; set; }
        public string Having { get; set; }
    }

    public class SelectJoin
    {
        public SelectJoinType Type { get; set; }
        public string Table { get; set; }
        public string On { get; set; }
    }

    public enum SelectJoinType
    {
        None,
        Left,
        Right
    }
}
