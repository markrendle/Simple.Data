using System.Collections.Generic;

namespace Shitty.Data.QueryPolyfills
{
    class ObjectValueResolver : ValueResolver
    {
        private readonly ObjectReference _reference;

        protected internal ObjectValueResolver(ObjectReference reference)
        {
            _reference = reference;
        }

        public override void CopyValue(IDictionary<string, object> source, IDictionary<string, object> target,
            IEnumerable<IDictionary<string, object>> sourceAggregationValues = null)
        {
            target[_reference.GetAliasOrName()] = GetValue(source);
        }

        public override object GetValue(IDictionary<string, object> source, IEnumerable<IDictionary<string, object>> sourceAggregationValues = null)
        {
            if (source == null) return null;
            if (_reference.HasOwner() && source.ContainsKey(_reference.GetOwner().GetName()))
            {
                var childDictionary = source[_reference.GetOwner().GetName()] as IDictionary<string, object>;
                return GetValue(childDictionary);
            }
            if (source.ContainsKey(_reference.GetName())) return source[_reference.GetName()];
            return null;
        }
    }
}