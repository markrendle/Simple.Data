using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Text;
using Simple.Data.Commands;

namespace Simple.Data
{
    /// <summary>
    /// Represents a qualified reference to a data store object, such as a table or column.
    /// </summary>
    public class ObjectReference : SimpleReference, IEquatable<ObjectReference>
    {
        private readonly string _name;
        private readonly ObjectReference _owner;
        private readonly DataStrategy _dataStrategy;

        internal ObjectReference(string name) : this(name, null, null, null)
        {
        }

        public ObjectReference(string name, ObjectReference owner) : this(name, owner, null, null)
        {
        }

        internal ObjectReference(string name, DataStrategy dataStrategy) : this(name, null, dataStrategy, null)
        {
        }

        internal ObjectReference(string name, ObjectReference owner, DataStrategy dataStrategy) : this(name, owner, dataStrategy, null)
        {
        }

        internal ObjectReference(string name, ObjectReference owner, DataStrategy dataStrategy, string alias) : base(alias)
        {
            _name = name;
            _owner = owner;
            _dataStrategy = dataStrategy;
        }

        /// <summary>
        /// Gets the owner of the referenced object; <c>null</c> if the object is top-level.
        /// </summary>
        /// <returns>The owner.</returns>
        public ObjectReference GetOwner()
        {
            return _owner;
        }

        public ObjectReference GetTop()
        {
            return ReferenceEquals(_owner, null) ? this : _owner.GetTop();
        }

        protected internal override DataStrategy FindDataStrategyInHierarchy()
        {
            return _dataStrategy ?? (ReferenceEquals(_owner, null) ? null : _owner.FindDataStrategyInHierarchy());
        }

        /// <summary>
        /// Gets the name of the referenced object.
        /// </summary>
        /// <returns>The name.</returns>
        public string GetName()
        {
            return _name;
        }

        public ObjectReference As(string alias)
        {
            return new ObjectReference(_name, _owner, _dataStrategy, alias);
        }

        public override string GetAliasOrName()
        {
            return GetAlias() ?? _name;
        }

        public override bool TryInvoke(InvokeBinder binder, object[] args, out object result)
        {
            if (_dataStrategy != null)
            {
                if (_dataStrategy.TryInvokeFunction(_name, () => binder.ArgumentsToDictionary(args), out result)) return true;
            }
            throw new InvalidOperationException();
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            if (base.TryInvokeMember(binder, args, out result)) return true;

            if (_dataStrategy != null)
            {
                // Probably a table...
                var table = new DynamicTable(_name, _dataStrategy);
                if (table.TryInvokeMember(binder, args, out result))
                {
                    _dataStrategy.SetMemberAsTable(this, table);
                    return true;
                }

                // Or it could be a schema reference...
                var schema = new DynamicSchema(_name, _dataStrategy);
                if (schema.TryInvokeMember(binder, args, out result))
                {
                    _dataStrategy.SetMemberAsSchema(this); 
                    return true;
                }
            }

            var dataStrategy = FindDataStrategyInHierarchy();
            if (dataStrategy != null)
            {
                var command = CommandFactory.GetCommandFor(binder.Name);
                if (command != null)
                {
                    var schema = dataStrategy.SetMemberAsSchema(_owner);
                    var table = schema.GetTable(_name);
                    result = command.Execute(dataStrategy, table, binder, args);
                }
                else
                {
                    if (dataStrategy.IsExpressionFunction(binder.Name, args))
                    {
                        result = new SimpleExpression(this, new SimpleFunction(binder.Name, args), SimpleExpressionType.Function);
                    }
                    else
                    {
                        result = new FunctionReference(binder.Name, this, args);
                    }
                }
                return true;
            }
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Provides the implementation for operations that get member values. Classes derived from the <see cref="T:System.Dynamic.DynamicObject"/> class can override this method to specify dynamic behavior for operations such as getting a value for a property.
        /// </summary>
        /// <param name="binder">Provides information about the object that called the dynamic operation. The binder.Name property provides the name of the member on which the dynamic operation is performed. For example, for the Console.WriteLine(sampleObject.SampleProperty) statement, where sampleObject is an instance of the class derived from the <see cref="T:System.Dynamic.DynamicObject"/> class, binder.Name returns "SampleProperty". The binder.IgnoreCase property specifies whether the member name is case-sensitive.</param>
        /// <param name="result">The result of the get operation. For example, if the method is called for a property, you can assign the property value to <paramref name="result"/>.</param>
        /// <returns>
        /// true if the operation is successful; otherwise, false. If this method returns false, the run-time binder of the language determines the behavior. (In most cases, a run-time exception is thrown.)
        /// </returns>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = new ObjectReference(binder.Name, this);
            return true;
        }

        public dynamic this[string name]
        {
            get { return new ObjectReference(name, this); }
        }

        /// <summary>
        /// Gets the names of all objects included in the reference as an array, with the uppermost object first.
        /// </summary>
        /// <returns></returns>
        public string[] GetAllObjectNames()
        {
            if (ReferenceEquals(GetOwner(), null)) return new[] {_name};
            return _owner.GetAllObjectNames().Concat(new[] {_name}).ToArray();
        }

        /// <summary>
        /// Gets the names of all objects included in the reference as an array, with the uppermost object first.
        /// </summary>
        /// <returns></returns>
        public Tuple<string,string>[] GetAllObjectNamesAndAliases()
        {
            if (ReferenceEquals(GetOwner(), null)) return new[] {Tuple.Create(_name, GetAlias())};
            return _owner.GetAllObjectNamesAndAliases().Concat(new[] {Tuple.Create(_name, GetAlias())}).ToArray();
        }

        public string GetAllObjectNamesDotted()
        {
            return string.Join(".", GetAllObjectNames());
        }

        public static ObjectReference FromString(string source)
        {
            return FromStrings(source.Split('.'));
        }

        public static ObjectReference FromStrings(params string[] source)
        {
            return source.Aggregate<string, ObjectReference>(null, (current, element) => new ObjectReference(element, current));
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(ObjectReference other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._name, _name) && Equals(other._owner, _owner);
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>. </param><filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (ObjectReference)) return false;
            return Equals((ObjectReference) obj);
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            unchecked
            {
                return ((_name != null ? _name.GetHashCode() : 0)*397) ^ (!ReferenceEquals(_owner, null) ? _owner.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            if (!ReferenceEquals(_owner, null))
            {
                return string.Concat(_owner, ".", _name);
            }
            return _name;
        }

        /// <summary>
        /// Implements the operator == to create a <see cref="SimpleExpression"/> with the type <see cref="SimpleExpressionType.Equal"/>.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <param name="value">The value.</param>
        /// <returns>The expression.</returns>
        public static SimpleExpression operator ==(ObjectReference column, object value)
        {
            return new SimpleExpression(column, value, SimpleExpressionType.Equal);
        }

        /// <summary>
        /// Implements the operator != to create a <see cref="SimpleExpression"/> with the type <see cref="SimpleExpressionType.NotEqual"/>.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <param name="value">The value.</param>
        /// <returns>The expression.</returns>
        public static SimpleExpression operator !=(ObjectReference column, object value)
        {
            return new SimpleExpression(column, value, SimpleExpressionType.NotEqual);
        }

        /// <summary>
        /// Implements the operator &lt; to create a <see cref="SimpleExpression"/> with the type <see cref="SimpleExpressionType.LessThan"/>.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <param name="value">The value.</param>
        /// <returns>The expression.</returns>
        public static SimpleExpression operator <(ObjectReference column, object value)
        {
            return new SimpleExpression(column, value, SimpleExpressionType.LessThan);
        }

        /// <summary>
        /// Implements the operator &gt; to create a <see cref="SimpleExpression"/> with the type <see cref="SimpleExpressionType.GreaterThan"/>.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <param name="value">The value.</param>
        /// <returns>The expression.</returns>
        public static SimpleExpression operator >(ObjectReference column, object value)
        {
            return new SimpleExpression(column, value, SimpleExpressionType.GreaterThan);
        }

        /// <summary>
        /// Implements the operator &lt;= to create a <see cref="SimpleExpression"/> with the type <see cref="SimpleExpressionType.LessThanOrEqual"/>.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <param name="value">The value.</param>
        /// <returns>The expression.</returns>
        public static SimpleExpression operator <=(ObjectReference column, object value)
        {
            return new SimpleExpression(column, value, SimpleExpressionType.LessThanOrEqual);
        }

        /// <summary>
        /// Implements the operator &gt;= to create a <see cref="SimpleExpression"/> with the type <see cref="SimpleExpressionType.GreaterThanOrEqual"/>.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <param name="value">The value.</param>
        /// <returns>The expression.</returns>
        public static SimpleExpression operator >=(ObjectReference column, object value)
        {
            return new SimpleExpression(column, value, SimpleExpressionType.GreaterThanOrEqual);
        }

        public static MathReference operator +(ObjectReference column, object value)
        {
            return new MathReference(column, value, MathOperator.Add);
        }

        public static MathReference operator -(ObjectReference column, object value)
        {
            return new MathReference(column, value, MathOperator.Subtract);
        }

        public static MathReference operator *(ObjectReference column, object value)
        {
            return new MathReference(column, value, MathOperator.Multiply);
        }

        public static MathReference operator /(ObjectReference column, object value)
        {
            return new MathReference(column, value, MathOperator.Divide);
        }

        public static MathReference operator %(ObjectReference column, object value)
        {
            return new MathReference(column, value, MathOperator.Modulo);
        }

        public bool HasOwner()
        {
            return !ReferenceEquals(null, _owner);
        }
    }
}
