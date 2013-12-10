namespace Simple.Data.Ado
{
    using System.Collections.Generic;
    using QueryPolyfills;

    class SingleRecordGraphNode : GraphNode
    {
        private IDictionary<string, object> _data;
        public SingleRecordGraphNode(string name) : base(name)
        {
        }

        public SingleRecordGraphNode(string name, GraphNode parent) : base(name, parent)
        {
        }

        public override void SetFields(IDictionary<string, object> data)
        {
            SetNewData(data);
            SendToChildren(data);
        }

        private void SendToChildren(IDictionary<string, object> data)
        {
            foreach (var child in Children)
            {
                child.SetFields(data);
            }
        }

        private void SetNewData(IDictionary<string, object> data)
        {
            var newData = CreateNewDataDictionary(data);
            if (newData == null) return;
            if (_data == null || !RecordsContainSameData(_data, newData))
            {
                _data = newData;
                if (Parent != null)
                {
                    Parent.SetField(Name, _data);
                }
                SendOnParentChanged();
            }
        }

        protected internal override void OnParentChanged()
        {
            _data = null;
            SendOnParentChanged();
        }

        protected internal override void SetField(string name, object value)
        {
            _data[name] = value;
        }

        public override IDictionary<string, object> GetResult()
        {
            return _data;
        }
    }
}