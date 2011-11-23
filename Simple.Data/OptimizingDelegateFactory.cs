using System;
using System.Collections.Generic;
using System.Linq;

namespace Simple.Data
{
    public abstract class OptimizingDelegateFactory
    {
        public virtual Func<object[], IDictionary<string, object>> CreateFindOneDelegate(Adapter adapter, string tableName, SimpleExpression criteria)
        {
            return adapter.CreateFindOneDelegate(tableName, criteria);
        }
    }
}