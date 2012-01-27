using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data
{
    /// <summary>
    /// Represents an explicit join in a <see cref="SimpleQuery"/>.
    /// There may be zero, one or multiple instances of this type in <see cref="SimpleQuery.Clauses"/>.
    /// </summary>
    public class JoinClause : SimpleQueryClauseBase
    {
        private readonly ObjectReference _table;
        private readonly JoinType _joinType;
        private readonly SimpleExpression _joinExpression;

        public JoinClause(ObjectReference table, JoinType joinType) : this(table, joinType, null)
        {
        }

        public JoinClause(ObjectReference table, SimpleExpression joinExpression) : this(table, JoinType.Inner, joinExpression)
        {
        }

        public JoinClause(ObjectReference table, JoinType joinType, SimpleExpression joinExpression)
        {
            if (table == null) throw new ArgumentNullException("table");
            _table = table;
            _joinType = joinType;
            _joinExpression = joinExpression;
        }

        public JoinType JoinType
        {
            get { return _joinType; }
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
            get { return _table.GetAlias() ?? _table.GetName(); }
        }
    }
}
