using Simple.Data.Operations;

namespace Simple.Data
{
    using System.Collections.Generic;
    using System.Linq;

    internal abstract class RunStrategy
    {
        protected abstract Adapter Adapter { get; }

        internal abstract OperationResult Execute(IOperation operation);

        protected static Dictionary<string, object> CreateChangedValuesDict(
            IEnumerable<KeyValuePair<string, object>> newValuesDict, IReadOnlyDictionary<string, object> originalValuesDict)
        {
            var changedValuesDict =
                newValuesDict.Where(
                    kvp =>
                    (!originalValuesDict.ContainsKey(kvp.Key)) || !(Equals(kvp.Value, originalValuesDict[kvp.Key])))
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            return changedValuesDict;
        }

        protected SimpleExpression CreateCriteriaFromOriginalValues(string tableName,
                                                                           IReadOnlyDictionary<string, object> newValuesDict,
                                                                           IReadOnlyDictionary<string, object>
                                                                               originalValuesDict)
        {
            var criteriaValues = Adapter.GetKey(tableName, originalValuesDict).ToDictionary();

            foreach (var kvp in originalValuesDict
                .Where(
                    originalKvp =>
                    newValuesDict.ContainsKey(originalKvp.Key) &&
                    !(Equals(newValuesDict[originalKvp.Key], originalKvp.Value))))
            {
                if (!criteriaValues.ContainsKey(kvp.Key))
                {
                    criteriaValues.Add(kvp);
                }
            };

            return ExpressionHelper.CriteriaDictionaryToExpression(tableName, criteriaValues);
        }
    }
}