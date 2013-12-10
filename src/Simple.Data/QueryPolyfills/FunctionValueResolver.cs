using System;
using System.Collections.Generic;

namespace Simple.Data.QueryPolyfills
{
    internal class FunctionValueResolver : ValueResolver
    {
        private readonly FunctionReference _reference;
        private readonly ValueResolver _argumentResolver;

        protected internal FunctionValueResolver(FunctionReference reference)
        {
            _reference = reference;
            _argumentResolver = Create(_reference.Argument);
        }

        public override void CopyValue(IDictionary<string, object> source, IDictionary<string, object> target,
            IEnumerable<IDictionary<string, object>> sourceAggregationValues = null)
        {
            target[_reference.GetAliasOrName()] = GetValue(source);
        }

        public override object GetValue(IDictionary<string, object> source, IEnumerable<IDictionary<string, object>> sourceAggregationValues = null)
        {
            return ApplyFunction(_argumentResolver.GetValue(source));
        }

        private object ApplyFunction(object value)
        {
            if (_reference.Name.Equals("length", StringComparison.OrdinalIgnoreCase))
            {
                return value == null ? 0 : value.ToString().Length;
            }
            return value;
        }
    }
}