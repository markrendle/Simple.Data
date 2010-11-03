using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace Simple.Data.Extensions
{
    internal static class BinderExtensions
    {
        public static bool HasSingleUnnamedArgument(this InvokeMemberBinder binder)
        {
            return binder.CallInfo.ArgumentCount == 1 &&
                (binder.CallInfo.ArgumentNames.Count == 0 || string.IsNullOrWhiteSpace(binder.CallInfo.ArgumentNames[0]));
        }
    }
}
