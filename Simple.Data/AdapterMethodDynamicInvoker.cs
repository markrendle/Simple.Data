using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data
{
    using System.Dynamic;
    using System.Reflection;

    internal class AdapterMethodDynamicInvoker
    {
        private readonly Adapter _adapter;

        public AdapterMethodDynamicInvoker(Adapter adapter)
        {
            _adapter = adapter;
        }

        public bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            var adapterMethods = _adapter.GetType().GetMethods().Where(m => m.Name == binder.Name).ToList();

            foreach (var method in adapterMethods)
            {
                var parameters = method.GetParameters().ToArray();
                if (parameters.Any(p => p.RawDefaultValue != DBNull.Value) && binder.CallInfo.ArgumentNames.Any(s => !string.IsNullOrWhiteSpace(s)))
                {
                    if (TryInvokeMemberWithNamedParameters(binder, args, out result, method, parameters))
                    {
                        return true;
                    }
                }
                else
                {
                    if (AreCompatible(parameters, args))
                    {
                        result = method.Invoke(_adapter, args);
                        return true;
                    }
                }
            }

            result = null;
            return false;
        }

        private bool TryInvokeMemberWithNamedParameters(InvokeMemberBinder binder, object[] args, out object result,
                                                        MethodInfo method, ParameterInfo[] parameters)
        {
            var fixedArgs = new List<object>();
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].RawDefaultValue == DBNull.Value)
                {
                    fixedArgs.Add(args[i]);
                }
                else
                {
                    var index = binder.CallInfo.ArgumentNames.IndexOf(parameters[i].Name);
                    if (index > -1)
                    {
                        if (!parameters[i].ParameterType.IsInstanceOfType(args[index]))
                        {
                            result = null;
                            return false;
                        }
                    }
                    else
                    {
                        fixedArgs.Add(parameters[i].RawDefaultValue);
                    }
                }
            }

            result = method.Invoke(_adapter, fixedArgs.ToArray());
            return true;
        }

        private static bool AreCompatible(IList<ParameterInfo> parameters, IList<object> args)
        {
            if (parameters.Count != args.Count) return false;
            for (int i = 0; i < parameters.Count; i++)
            {
                if (ReferenceEquals(args[i], null)) return !parameters[i].ParameterType.IsValueType;
                if (!parameters[i].ParameterType.IsInstanceOfType(args[i])) return false;
            }

            return true;
        }
    }
}
