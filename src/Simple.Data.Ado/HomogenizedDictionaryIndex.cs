using System.Collections.Generic;
using System.Linq;
using Simple.Data.Extensions;

namespace Simple.Data.Ado
{
    public class HomogenizedDictionaryIndex : OptimizedDictionaryIndex<string>
    {
        internal HomogenizedDictionaryIndex(IEnumerable<string> index) : base(index)
        {
        }

        internal HomogenizedDictionaryIndex(IDictionary<string, int> index) : base(index)
        {
        }

        public override bool ContainsKey(string key)
        {
            return base.ContainsKey(key.Homogenize());
        }

        public override int this[string key]
        {
            get
            {
                return base[key.Homogenize()];
            }
        }

        public override bool TryGetIndex(string key, out int index)
        {
            return base.TryGetIndex(key.Homogenize(), out index);
        }

        public static HomogenizedDictionaryIndex CreateIndex(IEnumerable<string> keys)
        {
            var index = keys.Select((key, i) => new KeyValuePair<string, int>(key.Homogenize(), i)).ToDictionary(kvp => kvp.Key,
                                                                                            kvp => kvp.Value);
            return new HomogenizedDictionaryIndex(index);
        }
    }
}
