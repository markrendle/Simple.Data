namespace Simple.Data
{
    public class OrderByClause : SimpleQueryClauseBase
    {
        private readonly ObjectReference _reference;
        private readonly OrderByDirection _direction;

        public OrderByClause(ObjectReference reference) : this(reference, OrderByDirection.Ascending)
        {
        }

        public OrderByClause(ObjectReference reference, OrderByDirection direction)
        {
            _reference = reference;
            _direction = direction;
        }

        public OrderByDirection Direction
        {
            get { return _direction; }
        }

        public ObjectReference Reference
        {
            get { return _reference; }
        }

    }
}