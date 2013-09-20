using System;
using System.Collections.Generic;
using System.Linq;
using Simple.Data.Operations;

namespace Simple.Data
{
    public abstract class OptimizingDelegateFactory
    {
        public virtual Func<object[], IDictionary<string, object>> CreateFindOneDelegate(Adapter adapter, string tableName, SimpleExpression criteria)
        {
            return adapter.CreateFindOneDelegate(tableName, criteria);
        }

        public virtual Func<object[], IDictionary<string,object>> CreateGetDelegate(Adapter adapter, GetOperation operation)
        {
            return args => adapter.Get(operation);
        }
    }

    internal class DefaultOptimizingDelegateFactory : OptimizingDelegateFactory
    {
        
    }
}