using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.Ado
{
    using System.ComponentModel.Composition;

    [Export(typeof(OptimizingDelegateFactory))]
    public class AdoOptimizingDelegateFactory : OptimizingDelegateFactory
    {
        public override Func<object[], IDictionary<string, object>> CreateFindOneDelegate(Adapter adapter, string tableName, SimpleExpression criteria)
        {
            return base.CreateFindOneDelegate(adapter, tableName, criteria);
        }
    }
}
