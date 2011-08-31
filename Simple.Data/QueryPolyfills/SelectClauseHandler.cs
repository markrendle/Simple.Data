namespace Simple.Data.QueryPolyfills
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    class SelectClauseHandler
    {
        private readonly IList<SimpleReference> _references;
        private readonly IList<ObjectReference> _objectReferences;
        private readonly IList<FunctionReference> _functionReferences; 
        private Func<int, IDictionary<string, object>> _creator;

        public SelectClauseHandler(SelectClause clause)
        {
            _references = clause.Columns.ToList();
            _objectReferences = _references.Select(r => r as ObjectReference).ToList();
            _functionReferences = _references.Select(r => r as FunctionReference).ToList();
        }

        public IEnumerable<IDictionary<string,object>> Run(IEnumerable<IDictionary<string,object>> source)
        {
            return source.Select(Run);
        }

        private IDictionary<string,object> Run(IDictionary<string,object> source)
        {
            if (_creator == null) _creator = CreateCreator(source);
            var target = _creator(_references.Count);
            for (int i = 0; i < _references.Count; i++)
            {
                bool _ = TryCopyAsObjectReference(_objectReferences[i], source, target)
                         || TryCopyAsFunctionReference(_functionReferences[i], source, target);
            }
            return target;
        }
        
        private static bool TryCopyAsObjectReference(ObjectReference reference, IDictionary<string, object> source, IDictionary<string, object> target)
        {
            if (reference.IsNull()) return false;
            target[reference.GetAliasOrName()] = source[reference.GetName()];
            return true;
        }

        private static bool TryCopyAsFunctionReference(FunctionReference reference, IDictionary<string, object> source, IDictionary<string, object> target)
        {
            if (reference.IsNull()) return false;
            var argument = (ObjectReference) reference.Argument;
            target[reference.GetAliasOrName()] = ApplyFunction(reference, source[argument.GetName()]);
            return true;
        }

        private static object ApplyFunction(FunctionReference function, object value)
        {
            if (function.Name.Equals("length", StringComparison.OrdinalIgnoreCase))
            {
                return value == null ? 0 : value.ToString().Length;
            }
            return value;
        }

        private Func<int, IDictionary<string, object>> CreateCreator(IDictionary<string, object> source)
        {
            var dictionary = source as Dictionary<string, object>;
            if (dictionary != null) return cap => new Dictionary<string, object>(cap, dictionary.Comparer);
            var sortedDictionary = source as SortedDictionary<string, object>;
            if (sortedDictionary != null) return cap => new SortedDictionary<string, object>(sortedDictionary.Comparer);
            var concurrentDictionary = source as ConcurrentDictionary<string, object>;
            if (concurrentDictionary != null) return cap => new ConcurrentDictionary<string, object>();

            var type = source.GetType();
            return cap => (IDictionary<string, object>) Activator.CreateInstance(type);
        }
    }
}