using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data
{
    public interface IAdapter
    {
        IDictionary<string, object> Find(string tableName, SimpleExpression criteria);
        IEnumerable<IDictionary<string, object>> FindAll(string tableName);
        IEnumerable<IDictionary<string, object>> FindAll(string tableName, SimpleExpression criteria);
        IDictionary<string, object> Insert(string tableName, IDictionary<string, object> data);
        int Update(string tableName, IDictionary<string, object> data, IDictionary<string, object> criteria);
        int Delete(string tableName, IDictionary<string, object> criteria);
    }
}
