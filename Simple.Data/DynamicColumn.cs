using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data
{
    public class DynamicColumn
    {
        private readonly string _tableName;
        private readonly string _name;

        public DynamicColumn(string tableName, string name)
        {
            _name = name;
            _tableName = tableName;
        }

        public string TableName
        {
            get { return _tableName; }
        }

        public string Name
        {
            get { return _name; }
        }
    }
}
