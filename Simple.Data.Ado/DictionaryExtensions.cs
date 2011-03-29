using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.Ado
{
    public static class DictionaryExtensions
    {
        public static object GetLockObject<TKey,TValue>(this IDictionary<TKey,TValue> dictionary)
        {
            var collection = dictionary as ICollection;
            if (collection != null)
            {
                return collection.SyncRoot;
            }
            return dictionary;
        }
    }
}
