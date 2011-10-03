namespace Simple.Data.QueryPolyfills
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    internal class WhereClauseHandler
    {
        private readonly Dictionary<SimpleExpressionType, Func<SimpleExpression, Func<IDictionary<string, object>, bool>>> _expressionFormatters;

                private readonly WhereClause _whereClause;

        public WhereClauseHandler(WhereClause whereClause)
        {
            _whereClause = whereClause;
            _expressionFormatters = new Dictionary<SimpleExpressionType, Func<SimpleExpression, Func<IDictionary<string,object>, bool>>>
                                        {
                                            {SimpleExpressionType.And, LogicalExpressionToWhereClause},
                                            {SimpleExpressionType.Or, LogicalExpressionToWhereClause},
                                            {SimpleExpressionType.Equal, EqualExpressionToWhereClause},
                                            {SimpleExpressionType.NotEqual, NotEqualExpressionToWhereClause},
                                            {SimpleExpressionType.Function, FunctionExpressionToWhereClause},
                                            {SimpleExpressionType.GreaterThan, GreaterThanToWhereClause},
                                            {SimpleExpressionType.LessThan, LessThanToWhereClause},
                                            {SimpleExpressionType.GreaterThanOrEqual, GreaterThanOrEqualToWhereClause},
                                            {SimpleExpressionType.LessThanOrEqual, LessThanOrEqualToWhereClause},
                                            {SimpleExpressionType.Empty, expr => _ => true },
                                        };
        }

        private Func<IDictionary<string, object>, bool> FunctionExpressionToWhereClause(SimpleExpression arg)
        {
            var key = GetKeyFromLeftOperand(arg);
            var function = arg.RightOperand as SimpleFunction;
            if (ReferenceEquals(function, null)) throw new InvalidOperationException("Expression type of function but no function supplied.");
            if (function.Name.Equals("like", StringComparison.OrdinalIgnoreCase))
            {
                var pattern = function.Args[0].ToString();
                if (pattern.Contains("%") || pattern.Contains("_")) // SQL Server LIKE
                {
                    pattern = pattern.Replace("%", ".*").Replace('_', '.');
                }

                var regex = new Regex("^" + pattern + "$", RegexOptions.Multiline | RegexOptions.IgnoreCase);

                return d => d.ContainsKey(key) && (!ReferenceEquals(d[key], null)) && regex.IsMatch(d[key].ToString());
            }

            throw new NotSupportedException("Expression Function not supported.");
        }

        private Func<IDictionary<string, object>, bool> GreaterThanToWhereClause(SimpleExpression arg)
        {
            var key = GetKeyFromLeftOperand(arg);

            return d => d.ContainsKey(key) && !ReferenceEquals(d[key], null) && ((IComparable)d[key]).CompareTo(arg.RightOperand) > 0;
        }

        private Func<IDictionary<string, object>, bool> LessThanToWhereClause(SimpleExpression arg)
        {
            var key = GetKeyFromLeftOperand(arg);

            return d => d.ContainsKey(key) && !ReferenceEquals(d[key], null) && ((IComparable)d[key]).CompareTo(arg.RightOperand) < 0;
        }

        private Func<IDictionary<string, object>, bool> GreaterThanOrEqualToWhereClause(SimpleExpression arg)
        {
            var key = GetKeyFromLeftOperand(arg);

            return d => d.ContainsKey(key) && !ReferenceEquals(d[key], null) && ((IComparable)d[key]).CompareTo(arg.RightOperand) >= 0;
        }

        private Func<IDictionary<string, object>, bool> LessThanOrEqualToWhereClause(SimpleExpression arg)
        {
            var key = GetKeyFromLeftOperand(arg);

            return d => d.ContainsKey(key) && !ReferenceEquals(d[key], null) && ((IComparable)d[key]).CompareTo(arg.RightOperand) <= 0;
        }

        private Func<IDictionary<string, object>, bool> NotEqualExpressionToWhereClause(SimpleExpression arg)
        {
            var key = GetKeyFromLeftOperand(arg);

            if (ReferenceEquals(arg.RightOperand, null))
            {
                return d => d.ContainsKey(key) && d[key] != null;
            }

            if (arg.RightOperand.GetType().IsArray)
            {
                return
                    d =>
                    d.ContainsKey(key) &&
                    !((IEnumerable)d[key]).Cast<object>().SequenceEqual(((IEnumerable)arg.RightOperand).Cast<object>());
            }

            return d => d.ContainsKey(key) && !Equals(d[key], arg.RightOperand);
        }

        private Func<IDictionary<string, object>, bool> EqualExpressionToWhereClause(SimpleExpression arg)
        {
            var key = GetKeyFromLeftOperand(arg);

            if (ReferenceEquals(arg.RightOperand, null))
            {
                return d => (!d.ContainsKey(key)) || d[key] == null;
            }

            if (arg.RightOperand.GetType().IsArray)
            {
                return
                    d =>
                    d.ContainsKey(key) &&
                    ((IEnumerable) d[key]).Cast<object>().SequenceEqual(((IEnumerable) arg.RightOperand).Cast<object>());
            }

            return d => d.ContainsKey(key) && Equals(d[key], arg.RightOperand);
        }

        private static string GetKeyFromLeftOperand(SimpleExpression arg)
        {
            var reference = arg.LeftOperand as ObjectReference;

            if (reference.IsNull()) throw new NotSupportedException("Only ObjectReference types are supported.");

            var key = reference.GetName();
            return key;
        }

        private Func<IDictionary<string,object>, bool> Format(SimpleExpression expression)
        {
            Func<SimpleExpression, Func<IDictionary<string,object>,bool>> formatter;

            if (_expressionFormatters.TryGetValue(expression.Type, out formatter))
            {
                return formatter(expression);
            }

            return _ => true;
        }

        private Func<IDictionary<string, object>, bool> LogicalExpressionToWhereClause(SimpleExpression arg)
        {
            var left = Format((SimpleExpression) arg.LeftOperand);
            var right = Format((SimpleExpression) arg.RightOperand);

            if (arg.Type == SimpleExpressionType.Or)
            {
                return d => (left(d) || right(d));
            }
            return d => (left(d) && right(d));
        }

        public IEnumerable<IDictionary<string, object>> Run(IEnumerable<IDictionary<string, object>> source)
        {
            var predicate = Format(_whereClause.Criteria);
            return source.Where(predicate);
        }
    }
}