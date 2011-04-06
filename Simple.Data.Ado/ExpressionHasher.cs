using System.Collections;
using System.Linq;

namespace Simple.Data.Ado
{
    class ExpressionHasher : ExpressionFormatterBase
    {
        protected override string FormatObject(object value)
        {
            var reference = value as DynamicReference;
            return !ReferenceEquals(reference, null) ? reference.ToString() : "?";
        }

        protected override string FormatRange(IRange range)
        {
            return "? AND ?";
        }

        protected override string FormatList(IEnumerable list)
        {
            return string.Format("({0})",
                                 string.Join(",", list.Cast<object>().Select(o => "?")));
        }
    }
}