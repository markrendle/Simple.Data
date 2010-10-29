using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data
{
    /// <summary>
    /// Provides a contract for adapters to persist data to data storage systems.
    /// Authors may implement this interface to create support for all kinds of databases or data stores.
    /// </summary>
    public interface IAdapter
    {
        IEnumerable<IDictionary<string, object>> Find(string tableName, SimpleExpression criteria);
        IDictionary<string, object> Insert(string tableName, IDictionary<string, object> data);
        int Update(string tableName, IDictionary<string, object> data, IDictionary<string, object> criteria);
        int Delete(string tableName, IDictionary<string, object> criteria);
    }
}
