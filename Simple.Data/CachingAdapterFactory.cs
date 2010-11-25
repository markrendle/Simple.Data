using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data
{
    class CachingAdapterFactory : AdapterFactory
    {
        private readonly ConcurrentDictionary<string, Adapter> _cache = new ConcurrentDictionary<string, Adapter>();
        public override Adapter Create(string adapterName, IEnumerable<KeyValuePair<string, object>> settings)
        {
            return _cache.GetOrAdd(HashSettings(adapterName, settings), s => DoCreate(adapterName, settings));
        }

        private static string HashSettings(string adapterName, IEnumerable<KeyValuePair<string, object>> settings)
        {
            return adapterName +
                       string.Join("#", settings.Select(kvp => kvp.Key + kvp.Value));
        }
    }
}
