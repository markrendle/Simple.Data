using System;
using System.Collections.Generic;
using System.Linq;

namespace Shitty.Data
{
    public abstract class OptimizingDelegateFactory
    {
        public virtual Func<object[], IDictionary<string, object>> CreateFindOneDelegate(Adapter adapter, string tableName, SimpleExpression criteria)
        {
            return adapter.CreateFindOneDelegate(tableName, criteria);
        }

        public virtual Func<object[], IDictionary<string,object>> CreateGetDelegate(Adapter adapter, string tableName, object[] keyValues)
        {
            return args => adapter.Get(tableName, args);
        }
    }

    internal class DefaultOptimizingDelegateFactory : OptimizingDelegateFactory
    {
        
    }
}