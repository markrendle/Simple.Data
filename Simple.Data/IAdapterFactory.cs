using System.Collections.Generic;

namespace Shitty.Data
{
    internal interface IAdapterFactory
    {
        Adapter Create(object settings);
        Adapter Create(string adapterName, object settings);
        Adapter Create(IEnumerable<KeyValuePair<string, object>> settings);
        Adapter Create(string adapterName, IEnumerable<KeyValuePair<string, object>> settings);
    }
}