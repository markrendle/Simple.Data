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
            var reference = value as ObjectReference;
            if (!ReferenceEquals(reference, null))
            {
                var table = _schema.FindTable(reference.GetOwner().ToString());
                var tableName = string.IsNullOrWhiteSpace(reference.GetOwner().Alias)
                                    ? table.QualifiedName
                                    : _schema.QuoteObjectName(reference.GetOwner().Alias);
                return tableName + "." + table.FindColumn(reference.GetName()).QuotedName;
            }

            return _commandBuilder.AddParameter(value, GetColumn(otherOperand as ObjectReference)).Name;
        }

        protected override string FormatRange(IRange range, object otherOperand)
        {
            return _commandBuilder.AddParameter(range, GetColumn(otherOperand as ObjectReference)).Name;
        }

        protected override string FormatList(IEnumerable list, object otherOperand)
        {
            return _commandBuilder.AddParameter(list, GetColumn(otherOperand as ObjectReference)).Name;
        }

        private Column GetColumn(ObjectReference reference)
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
