using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace Simple.Data.Commands
{
    using System.Reflection;
    using Extensions;

    class FindByCommand : ICommand
    {
        public bool IsCommandFor(string method)
        {
            return method.StartsWith("FindBy") || method.StartsWith("find_by_", StringComparison.InvariantCultureIgnoreCase);
        }

        public Func<object[], object> CreateDelegate(DataStrategy dataStrategy, DynamicTable table, InvokeMemberBinder binder, object[] args)
        {
            if (dataStrategy is SimpleTransaction) return null;

            if (binder.Name.Equals("FindBy") || binder.Name.Equals("find_by"))
            {
                if (args.Length == 0) throw new ArgumentException("FindBy requires arguments.");
                if (args.Length == 1)
                {
                    if (ReferenceEquals(args[0], null)) throw new ArgumentException("FindBy does not accept unnamed null argument.");
                    if (args[0].GetType().Namespace == null) return null;
                }
            }

            var criteriaDictionary = CreateCriteriaDictionary(binder, args);
            if (criteriaDictionary == null) return null;

            var criteriaExpression = ExpressionHelper.CriteriaDictionaryToExpression(table.GetQualifiedName(), criteriaDictionary);
            try
            {
                var adapter = dataStrategy.GetAdapter();
                var func = adapter.OptimizingDelegateFactory.CreateFindOneDelegate(adapter, table.GetQualifiedName(), criteriaExpression);
                return a =>
                           {
                               var data = func(a);
                               return (data != null && data.Count > 0)
                                          ? new SimpleRecord(data, table.GetQualifiedName(), dataStrategy)
                                          : null;
                           };
            }
            catch (NotImplementedException)
            {
                return null;
            }
        }

        private static IEnumerable<KeyValuePair<string, object>> CreateCriteriaDictionary(InvokeMemberBinder binder, IList<object> args)
        {
            IDictionary<string, object> criteriaDictionary = null;
            if (binder.Name.Equals("FindBy") || binder.Name.Equals("find_by"))
            {
                if (args.Count == 0) throw new ArgumentException("FindBy requires arguments.");
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
            return criteriaDictionary;
        }

        public object Execute(DataStrategy dataStrategy, DynamicTable table, InvokeMemberBinder binder, object[] args)
        {
            var criteriaExpression = ExpressionHelper.CriteriaDictionaryToExpression(table.GetQualifiedName(),
                                                                                     CreateCriteriaDictionary(binder,
                                                                                                              args));
            var data = dataStrategy.FindOne(table.GetQualifiedName(), criteriaExpression);
            return data != null ? new SimpleRecord(data, table.GetQualifiedName(), dataStrategy) : null;
        }

        public object Execute(DataStrategy dataStrategy, SimpleQuery query, InvokeMemberBinder binder, object[] args)
        {
            var criteriaExpression = ExpressionHelper.CriteriaDictionaryToExpression(query.TableName,
                                                                                     CreateCriteriaDictionary(binder,
                                                                                                              args));
            query = query.Where(criteriaExpression).Take(1);
            return query.FirstOrDefault();
        }
    }
}
