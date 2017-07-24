using System.Collections;
using System.Linq;
using System.Text;
using Shitty.Data.Ado.Schema;

namespace Shitty.Data.Ado
{
    class ExpressionFormatter : ExpressionFormatterBase
    {
        private readonly ICommandBuilder _commandBuilder;
        private readonly DatabaseSchema _schema;
        private readonly SimpleReferenceFormatter _simpleReferenceFormatter;

        public ExpressionFormatter(ICommandBuilder commandBuilder, DatabaseSchema schema) : base(() => schema.Operators)
        {
            _commandBuilder = commandBuilder;
            _schema = schema;
            _simpleReferenceFormatter = new SimpleReferenceFormatter(_schema, _commandBuilder);
        }

        protected override string FormatObject(object value, object otherOperand)
        {
            var objectReference = value as SimpleReference;

            if (!ReferenceEquals(objectReference, null))
                return _simpleReferenceFormatter.FormatColumnClause(objectReference);

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
            if (ReferenceEquals(reference, null))
            {
                return null;
            }
            var table = _schema.FindTable(reference.GetOwner().ToString());
            return table.FindColumn(reference.GetName());
        }
    }
}
