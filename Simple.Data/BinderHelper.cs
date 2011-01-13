using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace Simple.Data
{
    static class BinderHelper
    {
        internal static IDictionary<string, object> NamedArgumentsToDictionary(this InvokeMemberBinder binder, IList<object> args)
        {
            var dict = new Dictionary<string, object>() as ICollection<KeyValuePair<string,object>>;

            var keyValuePairs = binder.CallInfo.ArgumentNames
                .Reverse()
                .Zip(args.Reverse(), (k, v) => new KeyValuePair<string, object>(k, v));

            foreach (var pair in keyValuePairs)
            {
                dict.Add(pair);
            }

            return dict as IDictionary<string,object>;
        }
    }
}
