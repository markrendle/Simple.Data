using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System.Collections;
using MongoDB.Bson.DefaultSerializer;
using System.Text.RegularExpressions;

namespace Simple.Data.MongoDb
{
    class ExpressionFormatter : IExpressionFormatter
    {
        private readonly Dictionary<string, Func<DynamicReference, SimpleFunction, QueryComplete>> _supportedFunctions;
            

        private readonly MongoAdapter _adapter;

        public ExpressionFormatter(MongoAdapter adapter)
        {
            _adapter = adapter;

            _supportedFunctions = new Dictionary<string, Func<DynamicReference, SimpleFunction, QueryComplete>>(StringComparer.InvariantCultureIgnoreCase)
            {
                { "like", HandleLike },
                { "startswith", HandleStartsWith },
                { "contains", HandleContains },
                { "endswith", HandleEndsWith }
            };
        }

        public QueryComplete Format(SimpleExpression expression)
        {
            switch (expression.Type)
            {
                case SimpleExpressionType.And:
                    return LogicalExpression(expression, (l, r) => Query.And(l,r));
                case SimpleExpressionType.Equal:
                    return EqualExpression(expression);
                case SimpleExpressionType.GreaterThan:
                    return BinaryExpression(expression, Query.GT);
                case SimpleExpressionType.GreaterThanOrEqual:
                    return BinaryExpression(expression, Query.GTE);
                case SimpleExpressionType.LessThan:
                    return BinaryExpression(expression, Query.LT);
                case SimpleExpressionType.LessThanOrEqual:
                    return BinaryExpression(expression, Query.LTE);
                case SimpleExpressionType.Function:
                    return FunctionExpression(expression);
                case SimpleExpressionType.NotEqual:
                    return NotEqualExpression(expression);
                case SimpleExpressionType.Or:
                    return LogicalExpression(expression, (l, r) => Query.Or(l,r));
            }

            throw new NotSupportedException();
        }

        private QueryComplete BinaryExpression(SimpleExpression expression, Func<string, BsonValue, QueryComplete> builder)
        {
            var fieldName = (string)FormatObject(expression.LeftOperand);
            var value = BsonValue.Create(FormatObject(expression.RightOperand));
            return builder(fieldName, value);
        }

        private QueryComplete EqualExpression(SimpleExpression expression)
        {
            var fieldName = (string)FormatObject(expression.LeftOperand);
            var range = expression.RightOperand as IRange;
            if (range != null)
            {
                return Query.And(
                    Query.GTE(fieldName, BsonValue.Create(range.Start)),
                    Query.LTE(fieldName, BsonValue.Create(range.End)));
            }

            var list = expression.RightOperand as IEnumerable;
            if (list != null & expression.RightOperand.GetType() != typeof(string))
                return Query.In(fieldName, new BsonArray(list.OfType<object>()));

            return Query.EQ(fieldName, BsonValue.Create(FormatObject(expression.RightOperand)));
        }

        private QueryComplete FunctionExpression(SimpleExpression expression)
        {
            var function = expression.RightOperand as SimpleFunction;
            if (function == null) throw new InvalidOperationException("Expected SimpleFunction as the right operand.");

            Func<DynamicReference, SimpleFunction, QueryComplete> handler;
            if(!_supportedFunctions.TryGetValue(function.Name, out handler))
                throw new NotSupportedException(string.Format("Unknown function '{0}'.", function.Name));

            return handler((DynamicReference)expression.LeftOperand, function);
        }

        private QueryComplete LogicalExpression(SimpleExpression expression, Func<QueryComplete, QueryComplete, QueryComplete> builder)
        {
            return builder(
                Format((SimpleExpression)expression.LeftOperand),
                Format((SimpleExpression)expression.RightOperand));
        }

        private QueryComplete NotEqualExpression(SimpleExpression expression)
        {
            var fieldName = (string)FormatObject(expression.LeftOperand);
            var range = expression.RightOperand as IRange;
            if (range != null)
            {
                return Query.Or(
                    Query.LTE(fieldName, BsonValue.Create(range.Start)),
                    Query.GTE(fieldName, BsonValue.Create(range.End)));
            }

            var list = expression.RightOperand as IEnumerable;
            if (list != null & expression.RightOperand.GetType() != typeof(string))
                return Query.NotIn(fieldName, new BsonArray(list.OfType<object>()));

            return Query.NE(fieldName, BsonValue.Create(FormatObject(expression.RightOperand)));
        }

        private object FormatObject(object operand)
        {
            var reference = operand as DynamicReference;
            if (!ReferenceEquals(reference, null))
            {
                return GetFullDynamicReference(reference);
            }
            return operand;
        }

        private string GetFullDynamicReference(DynamicReference reference)
        {
            var names = new Stack<string>();
            string name;
            while(!ReferenceEquals(reference.GetOwner(), null))
            {
                name = reference.GetName();
                name = name == "Id" || name == "id" ? "_id" : name;
                names.Push(name);
                
                reference = reference.GetOwner();
            }
            return string.Join(".", names.ToArray());
        }

        private QueryComplete HandleLike(DynamicReference reference, SimpleFunction function)
        {
            if (function.Args[0] is Regex)
                return Query.Matches((string)FormatObject(reference), new BsonRegularExpression((Regex)function.Args[0]));
            else if (function.Args[0] is string)
                return Query.Matches((string)FormatObject(reference), new BsonRegularExpression((string)FormatObject(function.Args[0])));

            throw new InvalidOperationException("Like can only be used with a string or Regex.");
        }

        private QueryComplete HandleStartsWith(DynamicReference reference, SimpleFunction function)
        {
            if(!(function.Args[0] is string)) throw new InvalidOperationException("StartsWith can only be used with a string.");
         
            return Query.Matches((string)FormatObject(reference), new BsonRegularExpression("^" + (string)function.Args[0] + ".*"));
        }

        private QueryComplete HandleContains(DynamicReference reference, SimpleFunction function)
        {
            if (!(function.Args[0] is string)) throw new InvalidOperationException("StartsWith can only be used with a string.");

            return Query.Matches((string)FormatObject(reference), new BsonRegularExpression("^.*" + (string)function.Args[0] + ".*$"));
        }

        private QueryComplete HandleEndsWith(DynamicReference reference, SimpleFunction function)
        {
            if (!(function.Args[0] is string)) throw new InvalidOperationException("StartsWith can only be used with a string.");

            return Query.Matches((string)FormatObject(reference), new BsonRegularExpression(".*" + (string)function.Args[0] + "$"));
        }
    }
}