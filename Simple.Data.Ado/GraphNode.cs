namespace Simple.Data.Ado
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    abstract class GraphNode
    {
        private static readonly string[] Delimiter = {"__"};
        private readonly GraphNode _parent;
        private readonly string _name;

        protected string Name
        {
            get { return _name; }
        }

        private readonly Dictionary<string,string> _fields = new Dictionary<string, string>();

        protected Dictionary<string, string> Fields
        {
            get { return _fields; }
        }

        private readonly List<GraphNode> _children = new List<GraphNode>();

        public List<GraphNode> Children
        {
            get { return _children; }
        }

        public GraphNode Parent
        {
            get { return _parent; }
        }

        protected GraphNode(string name) : this(name, null)
        {
        }

        protected GraphNode(string name, GraphNode parent)
        {
            _parent = parent;
            _name = name;
        }

        public void AddField(string field)
        {
            _fields.Add(field, field.Split(Delimiter, StringSplitOptions.RemoveEmptyEntries).Last());
        }

        public abstract void SetFields(IDictionary<string, object> data);

        public void AddNode(string path)
        {
            var node = Find(path.Split(Delimiter, StringSplitOptions.RemoveEmptyEntries));
            node.AddField(path);
        }

        public GraphNode Find(string[] path, int index = 0)
        {
            if (path.Length - 1 == index) return this;
            var child = _children.FirstOrDefault(n => n._name.Equals(path[index + 1]));
            if (child == null)
            {
                child = path[index].Equals("with1")
                    ? (GraphNode) new SingleRecordGraphNode(path[index + 1], this)
                    : new CollectionGraphNode(path[index + 1], this);
                _children.Add(child);
            }
            return child.Find(path, index + 2);
        }

        protected internal abstract void OnParentChanged();

        protected internal abstract void SetField(string name, object value);

        public abstract IDictionary<string, object> GetResult();

        protected IDictionary<string, object> CreateNewDataDictionary(IDictionary<string, object> data)
        {
            bool anyValues = false;
            IDictionary<string, object> newData = new Dictionary<string, object>(HomogenizedEqualityComparer.DefaultInstance);
            foreach (var field in Fields)
            {
                object obj;
                data.TryGetValue(field.Key, out obj);
                anyValues = (newData[field.Value] = obj) != null || anyValues;
            }
            return anyValues ? newData : null;
        }

        protected bool RecordsContainSameData(IDictionary<string, object> a, IDictionary<string, object> b)
        {
            foreach (var field in _fields.Values)
            {
                object aObj;
                object bObj;
                if (a.TryGetValue(field, out aObj) != b.TryGetValue(field, out bObj)) return false;
                if (!Equals(aObj, bObj)) return false;
            }

            return true;
        }

        protected void SendOnParentChanged()
        {
            foreach (var child in Children)
            {
                child.OnParentChanged();
            }
        }
    }
}