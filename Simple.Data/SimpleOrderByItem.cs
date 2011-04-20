namespace Simple.Data
{
    public class SimpleOrderByItem
    {
        private readonly DynamicReference _reference;
        private readonly OrderByDirection _direction;

        public SimpleOrderByItem(DynamicReference reference) : this(reference, OrderByDirection.Ascending)
        {
        }

        public SimpleOrderByItem(DynamicReference reference, OrderByDirection direction)
        {
            _reference = reference;
            _direction = direction;
        }

        public OrderByDirection Direction
        {
            get { return _direction; }
        }

        public DynamicReference Reference
        {
            get { return _reference; }
        }
    }
}