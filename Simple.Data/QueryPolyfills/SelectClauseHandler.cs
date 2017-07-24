namespace Shitty.Data.QueryPolyfills
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    class SelectClauseHandler
    {
        private readonly IList<SimpleReference> _references;
        private readonly IList<ValueResolver> _resolvers; 
        private Func<int, IDictionary<string, object>> _creator;
        private readonly GroupingHandler _groupingHandler;

        public SelectClauseHandler(SelectClause clause)
        {
            _references = clause.Columns.ToList();
            if (!(_references.Count == 1 && _references[0] is SpecialReference))
            {
                _resolvers = _references.Select(ValueResolver.Create).ToList();
                if (_resolvers.OfType<AggregateValueResolver>().Any())
                {
                    _groupingHandler =
                        new GroupingHandler(
                            _references.Where(ReferenceIsNotAggregateFunction).Select(r => r.GetAliasOrName()).ToArray());
                }
            }
        }

        public IEnumerable<IDictionary<string,object>> Run(IEnumerable<IDictionary<string,object>> source)
        {
            if (_groupingHandler != null)
            {
                var groupedSource = _groupingHandler.Group(source);
                return groupedSource.Select(Run);
            }

            if (_references.Count == 1)
            {
                if (_references[0] is CountSpecialReference)
                {
                    return new[] {new Dictionary<string, object> {{"", source.Count()}}};
                }

                if (_references[0] is ExistsSpecialReference)
                {
                    if (source.Any())
                    {
                        return new[] {new Dictionary<string, object> {{"", 1}}};
                    }
                    return Enumerable.Empty<IDictionary<string, object>>();
                }
            }
            return source.Select(Run);
        }

        public IDictionary<string, object> Run(IGrouping<IDictionary<string, object>, IDictionary<string, object>> grouping)
        {
            if (_creator == null) _creator = DictionaryCreatorFactory.CreateFunc(grouping.Key);
            var target = _creator(_references.Count);
            for (int i = 0; i < _references.Count; i++)
            {
                _resolvers[i].CopyValue(grouping.Key, target, grouping);
            }
            return target;
        }

        private IDictionary<string,object> Run(IDictionary<string,object> source)
        {
            if (_creator == null) _creator = DictionaryCreatorFactory.CreateFunc(source);
            var target = _creator(_references.Count);
            for (int i = 0; i < _references.Count; i++)
            {
                _resolvers[i].CopyValue(source, target);
            }
            return target;
        }

        private static bool ReferenceIsNotAggregateFunction(SimpleReference reference)
        {
            var functionReference = reference as FunctionReference;
            return ReferenceEquals(functionReference, null) || !FunctionHandlers.Exists(functionReference.Name);
        }
        
    }
}