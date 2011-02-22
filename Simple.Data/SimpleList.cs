using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;

using Simple.Data.Extensions;
using System.Collections;

namespace Simple.Data
{
    public class SimpleList : DynamicObject, IList<object>
    {
        private readonly List<object> _innerList;

        public object this[int index]
        {
            get { return _innerList[index]; }
            set { _innerList[index] = value; }
        }

        public int Count
        {
            get { return _innerList.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public SimpleList(IEnumerable<object> other)
        {
            _innerList = new List<object>(other);
        }

        public void Add(object item)
        {
            _innerList.Add(item);
        }

        public void Clear()
        {
            _innerList.Clear();
        }

        public bool Contains(object item)
        {
            return _innerList.Contains(item);
        }

        public void CopyTo(object[] array, int arrayIndex)
        {
            _innerList.CopyTo(array, arrayIndex);
        }

        public int IndexOf(object item)
        {
            return _innerList.IndexOf(item);
        }

        public void Insert(int index, object item)
        {
            _innerList.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _innerList.RemoveAt(index);
        }

        public bool Remove(object item)
        {
            return _innerList.Remove(item);
        }

        public IEnumerator<object> GetEnumerator()
        {
            return _innerList.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_innerList).GetEnumerator();
        }

        public dynamic ElementAt(int index)
        {
            return _innerList.ElementAt(index);
        }

        public dynamic ElementAtOrDefault(int index)
        {
            return _innerList.ElementAtOrDefault(index);
        }

        public dynamic First()
        {
            return _innerList.First();
        }

        public dynamic FirstOrDefault()
        {
            return _innerList.FirstOrDefault();
        }

        public dynamic Last()
        {
            return _innerList.Last();
        }

        public dynamic LastOrDefault()
        {
            return _innerList.LastOrDefault();
        }

        public dynamic Single()
        {
            return _innerList.Single();
        }

        public dynamic SingleOrDefault()
        {
            return _innerList.SingleOrDefault();
        }

        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            if (ConcreteCollectionTypeCreator.IsCollectionType(binder.Type))
            {
                if (ConcreteCollectionTypeCreator.TryCreate(binder.Type, this, out result))
                    return true;
            }
       
            return base.TryConvert(binder, out result);
        }
    }
}