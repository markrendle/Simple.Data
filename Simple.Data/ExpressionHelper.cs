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

        public static SimpleExpression CriteriaDictionaryToExpression(ObjectReference table, IEnumerable<KeyValuePair<string, object>> dictionary)
        {
            if (dictionary.Count() == 1)
            {
                return CriteriaPairToExpression(table, dictionary.Single());
            }

            return new SimpleExpression(CriteriaPairToExpression(table, dictionary.First()),
                                        CriteriaDictionaryToExpression(table, dictionary.Skip(1)),
                                        SimpleExpressionType.And);
        }

        private static SimpleExpression CriteriaPairToExpression(string tableName, KeyValuePair<string, object> pair)
        {
            return new SimpleExpression(new ObjectReference(pair.Key, new ObjectReference(tableName)), pair.Value, SimpleExpressionType.Equal);
        }

        private static SimpleExpression CriteriaPairToExpression(ObjectReference table, KeyValuePair<string, object> pair)
        {
            return new SimpleExpression(new ObjectReference(pair.Key, table), pair.Value, SimpleExpressionType.Equal);
        }
    }
}
