namespace Simple.Data
{
    using System;

    public class WithCountClause : SimpleQueryClauseBase
    {
        private readonly Action<int> _setCount;

        public WithCountClause(Action<int> setCount)
        {
            _setCount = setCount;
        }

        public Action<int> SetCount
        {
            get { return _setCount; }
        }
    }
}