using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data
{
    public class JoinClause : SimpleQueryClauseBase
    {
        private readonly ObjectReference _table;
        private readonly SimpleExpression _joinExpression;

        public JoinClause(ObjectReference table, SimpleExpression joinExpression)
        {
            _table = table;
            _joinExpression = joinExpression;
        }

        public SimpleExpression JoinExpression
        {
            get { return _joinExpression; }
        }

        public ObjectReference Table
        {
            get { return _table; }
        }

        public string Name
        {
            get { return _table.Alias ?? _table.GetName(); }
        }
    }
}
