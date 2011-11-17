using System;
using System.Collections.Generic;
using System.Linq;

namespace Simple.Data.QueryPolyfills
{
    internal class AggregateValueResolver : ValueResolver
    {
        private readonly FunctionReference _reference;
        private readonly ValueResolver _argumentResolver;
        private readonly Func<IEnumerable<object>, object> _handler;

        public AggregateValueResolver(FunctionReference reference)
        {
            _reference = reference;
            _argumentResolver = Create(reference.Argument);
            _handler = FunctionHandlers.Get(reference.Name);
        }

        public override void CopyValue(IDictionary<string, object> source, IDictionary<string, object> target,
                                       IEnumerable<IDictionary<string, object>> sourceAggregationValues = null)
        {
            target[_reference.GetAliasOrName()] = GetValue(source, sourceAggregationValues);
        }

        public override object GetValue(IDictionary<string, object> source, IEnumerable<IDictionary<string, object>> sourceAggregationValues = null)
        {
            if (sourceAggregationValues == null) throw new ArgumentNullException("sourceAggregationValues");
            return _handler(sourceAggregationValues.Select(d => _argumentResolver.GetValue(d)));
        }
    }
}