using System.Collections;
using System.Linq;
using System.Text;
using Simple.Data.Ado.Schema;

namespace Simple.Data.Ado
{
    class ExpressionFormatter : ExpressionFormatterBase
    {
        private readonly ICommandBuilder _commandBuilder;
        private readonly DatabaseSchema _schema;

        public ExpressionFormatter(ICommandBuilder commandBuilder, DatabaseSchema schema)
        {
            _commandBuilder = commandBuilder;
            _schema = schema;
        }

        protected override string FormatObject(object value, object otherOperand)
        {
            var reference = value as DynamicReference;
            if (!ReferenceEquals(reference, null))
            {
                var table = _schema.FindTable(reference.GetOwner().ToString());
                return table.QualifiedName + "." + table.FindColumn(reference.GetName()).QuotedName;
            }

            return _commandBuilder.AddParameter(value, GetColumn(otherOperand as DynamicReference)).Name;
        }

        protected override string FormatRange(IRange range, object otherOperand)
        {
            return string.Format("{0} AND {1}", _commandBuilder.AddParameter(range.Start, GetColumn(otherOperand as DynamicReference)),
                                 _commandBuilder.AddParameter(range.End, GetColumn(otherOperand as DynamicReference)));
        }

        protected override string FormatList(IEnumerable list, object otherOperand)
        {
            return string.Format("({0})",
                                 string.Join(",", list.Cast<object>().Select(o => _commandBuilder.AddParameter(o, GetColumn(otherOperand as DynamicReference)))));
        }

        private Column GetColumn(DynamicReference reference)
        {
            if (reference == null)
            {
                return null;
            }
            var table = _schema.FindTable(reference.GetOwner().ToString());
            return table.FindColumn(reference.GetName());
        }
    }
}
