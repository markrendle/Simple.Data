using System.Collections.Generic;

namespace Simple.Data
{
    using System;

    public class FunctionReference : SimpleReference, IEquatable<FunctionReference>
    {
        private static readonly HashSet<string> KnownFunctionNames = new HashSet<string>
                                                                         {
                                                                             "min", "max", "average", "length", "sum", "count",
                                                                         };

        private static readonly HashSet<string> AggregateFunctionNames = new HashSet<string>
                                                                             {
                                                                                 "min", "max", "average", "sum", "count",
                                                                             };
        private readonly string _name;
        private readonly SimpleReference _argument;
        private readonly bool _isAggregate;
        private readonly string _alias;

        internal FunctionReference(string name, SimpleReference argument)
        {
            _name = name;
            _argument = argument;
            _isAggregate = AggregateFunctionNames.Contains(name.ToLowerInvariant());
        }

        private FunctionReference(string name, SimpleReference argument, bool isAggregate, string alias)
        {
            _name = name;
            _argument = argument;
            _isAggregate = isAggregate;
            _alias = alias;
        }

        public FunctionReference As(string alias)
        {
            return new FunctionReference(_name, _argument, _isAggregate, alias);
        }

        public string Alias
        {
            get { return _alias; }
        }

        public bool IsAggregate
        {
            get { return _isAggregate; }
        }

        public string Name
        {
            get { return _name; }
        }

        public SimpleReference Argument
        {
            get { return _argument; }
        }

        public static bool TryCreate(string name, SimpleReference argument, out object functionReference)
        {
            if (!KnownFunctionNames.Contains(name.ToLowerInvariant()))
            {
                functionReference = null;
                return false;
            }

            functionReference = new FunctionReference(name, argument);
            return true;
        }

        /// <summary>
        /// Implements the operator == to create a <see cref="SimpleExpression"/> with the type <see cref="SimpleExpressionType.Equal"/>.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <param name="value">The value.</param>
        /// <returns>The expression.</returns>
        public static SimpleExpression operator ==(FunctionReference column, object value)
        {
            return new SimpleExpression(column, value, SimpleExpressionType.Equal);
        }

        /// <summary>
        /// Implements the operator != to create a <see cref="SimpleExpression"/> with the type <see cref="SimpleExpressionType.NotEqual"/>.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <param name="value">The value.</param>
        /// <returns>The expression.</returns>
        public static SimpleExpression operator !=(FunctionReference column, object value)
        {
            return new SimpleExpression(column, value, SimpleExpressionType.NotEqual);
        }

        /// <summary>
        /// Implements the operator &lt; to create a <see cref="SimpleExpression"/> with the type <see cref="SimpleExpressionType.LessThan"/>.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <param name="value">The value.</param>
        /// <returns>The expression.</returns>
        public static SimpleExpression operator <(FunctionReference column, object value)
        {
            return new SimpleExpression(column, value, SimpleExpressionType.LessThan);
        }

        /// <summary>
        /// Implements the operator &gt; to create a <see cref="SimpleExpression"/> with the type <see cref="SimpleExpressionType.GreaterThan"/>.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <param name="value">The value.</param>
        /// <returns>The expression.</returns>
        public static SimpleExpression operator >(FunctionReference column, object value)
        {
            return new SimpleExpression(column, value, SimpleExpressionType.GreaterThan);
        }

        /// <summary>
        /// Implements the operator &lt;= to create a <see cref="SimpleExpression"/> with the type <see cref="SimpleExpressionType.LessThanOrEqual"/>.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <param name="value">The value.</param>
        /// <returns>The expression.</returns>
        public static SimpleExpression operator <=(FunctionReference column, object value)
        {
            return new SimpleExpression(column, value, SimpleExpressionType.LessThanOrEqual);
        }

        /// <summary>
        /// Implements the operator &gt;= to create a <see cref="SimpleExpression"/> with the type <see cref="SimpleExpressionType.GreaterThanOrEqual"/>.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <param name="value">The value.</param>
        /// <returns>The expression.</returns>
        public static SimpleExpression operator >=(FunctionReference column, object value)
        {
            return new SimpleExpression(column, value, SimpleExpressionType.GreaterThanOrEqual);
        }

        public bool Equals(FunctionReference other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._name, _name) && Equals(other._argument, _argument) && other._isAggregate.Equals(_isAggregate) && Equals(other._alias, _alias);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (FunctionReference)) return false;
            return Equals((FunctionReference) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = (_name != null ? _name.GetHashCode() : 0);
                result = (result*397) ^ (_argument != null ? _argument.GetHashCode() : 0);
                result = (result*397) ^ _isAggregate.GetHashCode();
                result = (result*397) ^ (_alias != null ? _alias.GetHashCode() : 0);
                return result;
            }
        }
    }
}