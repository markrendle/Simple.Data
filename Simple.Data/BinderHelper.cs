using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using Simple.Data.Extensions;

namespace Simple.Data
{
    static class BinderHelper
    {
        internal static IDictionary<string, object> NamedArgumentsToDictionary(this InvokeMemberBinder binder, IEnumerable<object> args)
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

        public static IEnumerable<KeyValuePair<string, object>> ArgumentsToDictionary(this InvokeMemberBinder binder, IEnumerable<object> args)
        {
            return args.Reverse()
                .Zip(binder.CallInfo.ArgumentNames.Reverse().ExtendInfinite(), (v, k) => new KeyValuePair<string, object>(k, v))
                .Reverse()
                .Select((kvp, i) => kvp.Key == null ? new KeyValuePair<string, object>("_" + i.ToString(), kvp.Value) : kvp);
        }
    }
}
