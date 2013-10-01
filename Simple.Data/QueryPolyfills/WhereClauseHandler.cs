namespace Simple.Data.QueryPolyfills
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Extensions;

    internal class WhereClauseHandler
    {
        private readonly Dictionary<SimpleExpressionType, Func<SimpleExpression, Func<IDictionary<string, object>, bool>>> _expressionFormatters;

        private readonly string _mainTableName;
        private readonly WhereClause _whereClause;

        public WhereClauseHandler(string mainTableName, WhereClause whereClause)
        {
            _mainTableName = mainTableName;
            _whereClause = whereClause;
            _expressionFormatters = new Dictionary<SimpleExpressionType, Func<SimpleExpression, Func<IDictionary<string, object>, bool>>>
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
            var function = arg.RightOperand as SimpleFunction;
            if (ReferenceEquals(function, null)) throw new InvalidOperationException("Expression type of function but no function supplied.");
            if (function.Name.Equals("like", StringComparison.OrdinalIgnoreCase) ||
                function.Name.Equals("notlike", StringComparison.OrdinalIgnoreCase))
            {
                var pattern = function.Args[0].ToString();
                if (pattern.Contains("%") || pattern.Contains("_")) // SQL Server LIKE
                {
                    pattern = pattern.Replace("%", ".*").Replace('_', '.');
                }

                var regex = new Regex("^" + pattern + "$", RegexOptions.Multiline | RegexOptions.IgnoreCase);

                if (function.Name.Equals("like", StringComparison.OrdinalIgnoreCase))
                {
                    return d => Resolve(d, arg.LeftOperand).Count > 0 && Resolve(d, arg.LeftOperand).OfType<string>().Any(regex.IsMatch);
                }
                if (function.Name.Equals("notlike", StringComparison.OrdinalIgnoreCase))
                {
                    return d => Resolve(d, arg.LeftOperand).Count > 0 && Resolve(d, arg.LeftOperand).OfType<string>().All(input => !regex.IsMatch(input));
                }
            }

            throw new NotSupportedException("Expression Function not supported.");
        }

        private Func<IDictionary<string, object>, bool> GreaterThanToWhereClause(SimpleExpression arg)
        {
            return d => Resolve(d, arg.LeftOperand).OfType<IComparable>().Any(o => o.CompareTo(arg.RightOperand) > 0);
        }

        private Func<IDictionary<string, object>, bool> LessThanToWhereClause(SimpleExpression arg)
        {
            return d => Resolve(d, arg.LeftOperand).OfType<IComparable>().Any(o => o.CompareTo(arg.RightOperand) < 0);
        }

        private Func<IDictionary<string, object>, bool> GreaterThanOrEqualToWhereClause(SimpleExpression arg)
        {
            return d => Resolve(d, arg.LeftOperand).OfType<IComparable>().Any(o => o.CompareTo(arg.RightOperand) >= 0);
        }

        private Func<IDictionary<string, object>, bool> LessThanOrEqualToWhereClause(SimpleExpression arg)
        {
            return d => Resolve(d, arg.LeftOperand).OfType<IComparable>().Any(o => o.CompareTo(arg.RightOperand) <= 0);
        }

        private Func<IDictionary<string, object>, bool> NotEqualExpressionToWhereClause(SimpleExpression arg)
        {
            var equal = EqualExpressionToWhereClause(arg);
            return d => !equal(d);
        }

        private Func<IDictionary<string, object>, bool> EqualExpressionToWhereClause(SimpleExpression arg)
        {
            if (ReferenceEquals(arg.RightOperand, null))
            {
                return d => Resolve(d, arg.LeftOperand).Count == 0 || Resolve(d, arg.LeftOperand).Any(o => ReferenceEquals(o, null));
            }

            if (arg.RightOperand.GetType().IsArray)
            {
                return
                    d =>
                        {
                            var resolvedLeftOperand = Resolve(d, arg.LeftOperand);
                            if (resolvedLeftOperand.OfType<IEnumerable>().Any())
                            {
                                return resolvedLeftOperand.OfType<IEnumerable>().Any(
                                    o => o.Cast<object>().SequenceEqual(((IEnumerable)arg.RightOperand).Cast<object>()));
                            }
                            return resolvedLeftOperand.Any(
                                o => ((IEnumerable)arg.RightOperand).Cast<object>().Contains(o));
                        };
            }

            return d => Resolve(d, arg.LeftOperand).Contains(arg.RightOperand);
        }

        private Func<IDictionary<string, object>, bool> Format(SimpleExpression expression)
        {
            Func<SimpleExpression, Func<IDictionary<string, object>, bool>> formatter;

            if (_expressionFormatters.TryGetValue(expression.Type, out formatter))
            {
                return formatter(expression);
            }

            return _ => true;
        }

        private Func<IDictionary<string, object>, bool> LogicalExpressionToWhereClause(SimpleExpression arg)
        {
            var left = Format((SimpleExpression)arg.LeftOperand);
            var right = Format((SimpleExpression)arg.RightOperand);

            if (arg.Type == SimpleExpressionType.Or)
            {
                return d => (left(d) || right(d));
            }
            return d => (left(d) && right(d));
        }

        private IList<object> Resolve(IDictionary<string, object> dict, object operand, string key = null)
        {
            var objectReference = operand as ObjectReference;
            if (objectReference.IsNull()) return new object[0];

            key = key ?? objectReference.GetAliasOrName();
            var keys = objectReference.GetAllObjectNames();

            if (keys.Length > 2)
            {
                if (_mainTableName.Contains("."))
                {
                    keys = keys.Skip(1).ToArray();
                    keys[0] = _mainTableName;
                }
            }
            if (keys.Length > 2)
            {
                return ResolveSubs(dict, objectReference.GetOwner(), key).ToList();
            }

            if (keys.Length == 2 && !HomogenizedEqualityComparer.DefaultInstance.Equals(keys[0].Singularize(), _mainTableName.Singularize()))
            {
                var joinedDict = dict[keys[0]] as IDictionary<string, object>;
                if (joinedDict != null && joinedDict.ContainsKey(keys[1]))
                {
                    return new[] { joinedDict[keys[1]] };
                }

                var joinedDicts = dict[keys[0]] as IEnumerable<IDictionary<string, object>>;
                if (joinedDicts != null)
                {
                    return joinedDicts.Select(d => d.ContainsKey(keys[1]) ? d[keys[1]] : null).ToArray();
                }
            }

            if (dict.ContainsKey(key))
                return new[] { dict[key] };

            return new object[0];
        }

        private IEnumerable<object> ResolveSubs(IDictionary<string, object> dict, ObjectReference objectReference, string key)
        {
            if (objectReference.IsNull()) return Enumerable.Empty<object>();

            if (dict.ContainsKey(objectReference.GetName()))
            {
                var master = dict[objectReference.GetName()] as IDictionary<string, object>;
                if (master != null)
                {
                    if (master.ContainsKey(key))
                    {
                        return new[] { master[key] };
                    }
                }

                var detail = dict[objectReference.GetName()] as IEnumerable<IDictionary<string, object>>;
                if (detail != null)
                {
                    return detail.SelectMany(d => Resolve(d, objectReference, key));
                }
            }

            return ResolveSubs(dict, objectReference.GetOwner(), key);
        }

        public IEnumerable<IDictionary<string, object>> Run(IEnumerable<IDictionary<string, object>> source)
        {
            var predicate = Format(_whereClause.Criteria);
            return source.Where(predicate);
        }
    }
}