using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Shitty.Data.Extensions;

namespace Shitty.Data
{
    class AdapterFactory : IAdapterFactory
    {
        private readonly Composer _composer;

        protected AdapterFactory() : this(Composer.Default)
        {
        }

        protected AdapterFactory(Composer composer)
        {
            _composer = composer;
        }

        public Adapter Create(object settings)
        {
            return Create(settings.ObjectToDictionary());
        }

        public Adapter Create(string adapterName, object settings)
        {
            return Create(adapterName, settings.AnonymousObjectToDictionary());
        }

        public Adapter Create(IEnumerable<KeyValuePair<string,object>> settings)
        {
            if (settings.Any( kvp => kvp.Key.Equals("ConnectionString",StringComparison.OrdinalIgnoreCase)))
            {
                return Create("Ado", settings);
            }

            throw new ArgumentException("Cannot infer adapter type from settings.");
        }

        public virtual Adapter Create(string adapterName, IEnumerable<KeyValuePair<string, object>> settings)
        {
            return DoCreate(adapterName, settings);
        }

        protected Adapter DoCreate(string adapterName, IEnumerable<KeyValuePair<string, object>> settings)
        {
            var adapter = _composer.Compose<Adapter>(adapterName);
            adapter.Setup(settings);
            return adapter;
        }
    }
}
