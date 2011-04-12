using System.Collections;
using System.Linq;

namespace Simple.Data.Ado
{
    class ExpressionHasher : ExpressionFormatterBase
    {
        protected override string FormatObject(object value, object otherOperand)
        {
            var reference = value as DynamicReference;
            return !ReferenceEquals(reference, null) ? reference.ToString() : "?";
        }

        protected override string FormatRange(IRange range, object otherOperand)
        {
            return "? AND ?";
        }

        protected override string FormatList(IEnumerable list, object otherOperand)
        {
            return string.Format("({0})",
                                 string.Join(",", list.Cast<object>().Select(o => "?")));
        }
    }
}