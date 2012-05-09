using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.Commands
{
    using System.Dynamic;
    using Extensions;

    internal static class ArgumentHelper
    {
        internal static void CheckFindArgs(object[] args, InvokeMemberBinder binder)
        {
            if (args.Length == 0) throw new ArgumentException(binder.Name + "requires arguments.");
            if (args.Length == 1)
            {
                if (ReferenceEquals(args[0], null) && binder.CallInfo.ArgumentNames.Count == 0)
                    throw new ArgumentException(binder.Name + " does not accept unnamed null argument.");
            }
        }

        internal static IEnumerable<KeyValuePair<string, object>> CreateCriteriaDictionary(InvokeMemberBinder binder, IList<object> args, params string[] exactNames)
        {
            IDictionary<string, object> criteriaDictionary = null;
            if (exactNames.Contains(binder.Name))
            {
                if (binder.CallInfo.ArgumentNames != null && binder.CallInfo.ArgumentNames.Count > 0)
                {
                    criteriaDictionary = binder.NamedArgumentsToDictionary(args);
                }
                else if (args.Count == 1)
                {
                    if (ReferenceEquals(args[0], null)) throw new ArgumentException("FindBy does not accept unnamed null argument.");
                    criteriaDictionary = args[0].ObjectToDictionary();
                }
            }
            else
            {
                criteriaDictionary = MethodNameParser.ParseFromBinder(binder, args);
            }

            if (criteriaDictionary == null || criteriaDictionary.Count == 0)
            {
                throw new ArgumentException(binder.Name + " requires an equal number of column names and values to filter data by.");
            }
            return criteriaDictionary;
        }
    }
}
