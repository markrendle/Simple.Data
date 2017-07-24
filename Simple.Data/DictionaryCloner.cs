namespace Shitty.Data
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public class DictionaryCloner
    {
        public IDictionary<string, object> CloneDictionary(IDictionary<string,object> source)
        {
            var cloneable = source as ICloneable;
            if (cloneable != null)
            {
                return (IDictionary<string, object>)cloneable.Clone();
            }

            var dictionary = source as Dictionary<string, object>;
            if (dictionary != null)
            {
                return CloneSystemDictionary(dictionary);
            }

            return CloneCustomDictionary(source);
        }

        private IDictionary<string, object> CloneCustomDictionary(IDictionary<string, object> source)
        {
            var clone = Activator.CreateInstance(source.GetType()) as IDictionary<string, object>;
            if (clone == null) throw new InvalidOperationException("Internal data structure cannot be cloned.");
            CopyDictionaryAndCloneNestedDictionaries(source, clone);
            return clone;
        }

        private IDictionary<string, object> CloneSystemDictionary(Dictionary<string, object> dictionary)
        {
            var clone = new Dictionary<string, object>(dictionary.Count, dictionary.Comparer);
            CopyDictionaryAndCloneNestedDictionaries(dictionary, clone);
            return clone;
        }

        private void CopyDictionaryAndCloneNestedDictionaries(IEnumerable<KeyValuePair<string, object>> dictionary, IDictionary<string, object> clone)
        {
            foreach (var keyValuePair in dictionary)
            {
                clone.Add(keyValuePair.Key, CloneValue(keyValuePair.Value));
            }
        }

        public object CloneValue(object source)
        {
            if (ReferenceEquals(source, null)) return null;

            var nestedDictionaries = source as IEnumerable<IDictionary<string, object>>;
            if (nestedDictionaries != null)
            {
                return CopyNestedDictionaryList(nestedDictionaries, source.GetType());
            }

            var nestedDictionary = source as IDictionary<string, object>;
            return nestedDictionary != null ? CloneDictionary(nestedDictionary) : source;
        }

        public object CopyNestedDictionaryList(IEnumerable<IDictionary<string, object>> nestedDictionaries, Type collectionType)
        {
            var collection = Activator.CreateInstance(collectionType) as IList ?? new List<IDictionary<string,object>>();

            foreach (var nestedDictionary1 in nestedDictionaries)
            {
                collection.Add(CloneDictionary(nestedDictionary1));
            }
            return collection;
        }
    }
}