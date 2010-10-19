using System.Collections.Generic;
using System.Linq;

namespace Simple.Data
{
    class ExpressionHelper
    {
        public static SimpleExpression CriteriaDictionaryToExpression(string tableName, IEnumerable<KeyValuePair<string, object>> dictionary)
        {
            if (dictionary.Count() == 1)
            {
                return CriteriaPairToExpression(tableName, dictionary.Single());
            }
            
            return new SimpleExpression(CriteriaPairToExpression(tableName, dictionary.First()),
                                        CriteriaDictionaryToExpression(tableName, dictionary.Skip(1)),
                                        SimpleExpressionType.And);
        }

        private static SimpleExpression CriteriaPairToExpression(string tableName, KeyValuePair<string, object> pair)
        {
            return new SimpleExpression(new DynamicReference(pair.Key, new DynamicReference(tableName)), pair.Value, SimpleExpressionType.Equal);
        }
    }
}
