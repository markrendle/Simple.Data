using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace Simple.Data
{
    static class BinderHelper
    {
        internal static Dictionary<string, object> NamedArgumentsToDictionary(InvokeMemberBinder binder, IList<object> args)
        {
            var dict = new Dictionary<string, object>();

            for (int i = 0; i < args.Count; i++)
            {
                dict.Add(binder.CallInfo.ArgumentNames[i], args[i]);
            }

            return dict;
        }
    }
}
