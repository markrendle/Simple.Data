namespace Simple.Data.Ado
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using QueryPolyfills;

    class CollectionGraphNode : GraphNode
    {
        private IList<IDictionary<string, object>> _data = new List<IDictionary<string, object>>();
        private IDictionary<string, object> _current; 

        public CollectionGraphNode(string name) : base(name)
        {
            throw new NotImplementedException();
        }

        public CollectionGraphNode(string name, GraphNode parent) : base(name, parent)
        {
        }

        public override void SetFields(IDictionary<string, object> data)
        {
            var newData = CreateNewDataDictionary(data);
            if (newData == null) return;
            if (!_data.Any(d => RecordsContainSameData(d, newData)))
            {
                _current = newData;
                _data.Add(newData);
            }

            foreach (var child in Children)
            {
                child.SetFields(data);
            }
        }

        protected internal override void OnParentChanged()
        {
            _data = new List<IDictionary<string, object>>();
            Parent.SetField(Name, _data);
            _current = null;

            SendOnParentChanged();
        }

        protected internal override void SetField(string name, object value)
        {
            if (_current == null)
            {
                _current = new Dictionary<string, object>();
                _data.Add(_current);
            }
            _current[name] = value;
        }

        public override IDictionary<string, object> GetResult()
        {
            throw new InvalidOperationException("Calling GetResult is not valid on a Collection node.");
        }
    }
}