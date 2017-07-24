using System.Collections.Generic;
using System.Linq;

namespace Shitty.Data.QueryPolyfills
{
    internal class OrderByClauseHandler
    {
        private readonly OrderByClause _orderByClause;

        public OrderByClauseHandler(OrderByClause orderByClause)
        {
            _orderByClause = orderByClause;
        }

        public IEnumerable<IDictionary<string, object>> Run(IEnumerable<IDictionary<string, object>> source)
        {
            return _orderByClause.Direction == OrderByDirection.Ascending
                       ? source.OrderBy(d => d[_orderByClause.Reference.GetName()])
                       : source.OrderByDescending(d => d[_orderByClause.Reference.GetName()]);
        }
    }
}