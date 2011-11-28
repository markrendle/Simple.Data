using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.QueryPolyfills
{
    abstract class ValueResolver
    {
        public static ValueResolver Create(SimpleReference reference)
        {
            var objectReference = reference as ObjectReference;
            if (!ReferenceEquals(null, objectReference)) return new ObjectValueResolver(objectReference);
            var functionReference = reference as FunctionReference;
            if (!ReferenceEquals(null, functionReference))
            {
                if (FunctionHandlers.Exists(functionReference.Name)) return new AggregateValueResolver(functionReference);
                return new FunctionValueResolver(functionReference);
            }

            throw new InvalidOperationException("Unresolvable Reference type.");
        }

        public abstract void CopyValue(IDictionary<string, object> source, IDictionary<string, object> target, IEnumerable<IDictionary<string,object>> sourceAggregationValues = null);

        public abstract object GetValue(IDictionary<string, object> source, IEnumerable<IDictionary<string, object>> sourceAggregationValues = null);
    }
}
