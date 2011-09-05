using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data
{
    class CachingAdapterFactory : AdapterFactory
    {
        public CachingAdapterFactory()
        {
            
        }

        public CachingAdapterFactory(Composer composer) : base(composer)
        {
            
        }

        private readonly ConcurrentDictionary<string, Adapter> _cache = new ConcurrentDictionary<string, Adapter>();
        public override Adapter Create(string adapterName, IEnumerable<KeyValuePair<string, object>> settings)
        {
            List<KeyValuePair<string, object>> mat;
            string hash;
            if (settings == null)
            {
                mat = new List<KeyValuePair<string, object>>();
                hash = adapterName;
            }
            else
            {
                mat = settings.ToList();
                hash = HashSettings(adapterName, mat);
            }
            
            return _cache.GetOrAdd(hash, _ => DoCreate(adapterName, mat));
        }

        private static string HashSettings(string adapterName, IEnumerable<KeyValuePair<string, object>> settings)
        {
            return adapterName +
                       string.Join("#", settings.Select(kvp => kvp.Key + "=" + kvp.Value));
        }

        public void Reset()
        {
            _cache.Clear();
        }
    }
}
