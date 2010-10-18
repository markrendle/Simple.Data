using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace Simple.Data
{
    public class DynamicReference : DynamicObject
    {
        private readonly string _name;
        private readonly DynamicReference _owner;

        internal DynamicReference(string name) : this(name, null)
        {
        }

        internal DynamicReference(string name, DynamicReference owner)
        {
            _name = name;
            _owner = owner;
        }

        public DynamicReference Owner
        {
            get { return _owner; }
        }

        public string Name
        {
            get { return _name; }
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = new DynamicReference(binder.Name, this);
            return true;
        }

        public string[] GetAllObjectNames()
        {
            if (ReferenceEquals(Owner, null)) return new[] {_name};
            return _owner.GetAllObjectNames().Concat(new[] {_name}).ToArray();
        }

        public static DynamicReference FromString(string source)
        {
            return FromStrings(source.Split('.'));
        }

        public static DynamicReference FromStrings(params string[] source)
        {
            return source.Aggregate<string, DynamicReference>(null, (current, element) => new DynamicReference(element, current));
        }

        public static SimpleExpression operator ==(DynamicReference column, object value)
        {
            return new SimpleExpression(column, value, SimpleExpressionType.Equal);
        }

        public static SimpleExpression operator !=(DynamicReference column, object value)
        {
            return new SimpleExpression(column, value, SimpleExpressionType.NotEqual);
        }

        public static SimpleExpression operator <(DynamicReference column, object value)
        {
            return new SimpleExpression(column, value, SimpleExpressionType.LessThan);
        }

        public static SimpleExpression operator >(DynamicReference column, object value)
        {
            return new SimpleExpression(column, value, SimpleExpressionType.GreaterThan);
        }

        public static SimpleExpression operator <=(DynamicReference column, object value)
        {
            return new SimpleExpression(column, value, SimpleExpressionType.LessThanOrEqual);
        }

        public static SimpleExpression operator >=(DynamicReference column, object value)
        {
            return new SimpleExpression(column, value, SimpleExpressionType.GreaterThanOrEqual);
        }
    }
}
