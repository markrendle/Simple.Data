using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data
{
    partial class DynamicRecord : IDictionary<string, object>
    {
        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item)
        {
            _data.Add(item);
        }

        void ICollection<KeyValuePair<string, object>>.Clear()
        {
            _data.Clear();
        }

        bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item)
        {
            return _data.Contains(item);
        }

        void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            _data.CopyTo(array, arrayIndex);
        }

        bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
        {
            return _data.Remove(item);
        }

        int ICollection<KeyValuePair<string, object>>.Count
        {
            get { return _data.Count; }
        }

        bool ICollection<KeyValuePair<string, object>>.IsReadOnly
        {
            get { return _data.IsReadOnly; }
        }

        bool IDictionary<string, object>.ContainsKey(string key)
        {
            return _data.ContainsKey(key);
        }

        void IDictionary<string, object>.Add(string key, object value)
        {
            _data.Add(key, value);
        }

        bool IDictionary<string, object>.Remove(string key)
        {
            return _data.Remove(key);
        }

        bool IDictionary<string, object>.TryGetValue(string key, out object value)
        {
            return _data.TryGetValue(key, out value);
        }

        object IDictionary<string, object>.this[string key]
        {
            get { return _data[key]; }
            set { _data[key] = value; }
        }

        ICollection<string> IDictionary<string, object>.Keys
        {
            get { return _data.Keys; }
        }

        ICollection<object> IDictionary<string, object>.Values
        {
            get { return _data.Values; }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _data.GetEnumerator();
        }
    }
}
