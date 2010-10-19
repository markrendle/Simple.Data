using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Simple.Data.Ado;

namespace Simple.Data.Commands
{
    class Joiner
    {
        private readonly ISchemaProvider _schemaProvider;
        private readonly ConcurrentDictionary<string, string> _done = new ConcurrentDictionary<string, string>();

        public Joiner(ISchemaProvider schemaProvider)
        {
            if (schemaProvider == null) throw new ArgumentNullException("schemaProvider");
            _schemaProvider = schemaProvider;
        }

        public void AddJoins(string mainTableName, CommandBuilder commandBuilder, SimpleExpression expression)
        {
            _done.AddOrUpdate(mainTableName, string.Empty, (s, o) => string.Empty);
            foreach (var tablePair in GetTablePairs(expression))
            {
                AddJoin(tablePair.Item1, tablePair.Item2);
            }
        }

        private void AddJoin(string table1, string table2)
        {
        }

        private IEnumerable<Tuple<string,string>> GetTablePairs(SimpleExpression expression)
        {
            return GetReferencesFromExpression(expression)
                .SelectMany(dr => dr.GetAllObjectNames().SkipLast().ToTuplePairs())
                .Distinct();
        }

        private IEnumerable<DynamicReference> GetReferencesFromExpression(SimpleExpression expression)
        {
            if (expression.Type == SimpleExpressionType.And || expression.Type == SimpleExpressionType.Or)
            {
                return GetReferencesFromExpression((SimpleExpression) expression.LeftOperand)
                    .Concat(GetReferencesFromExpression((SimpleExpression) expression.LeftOperand));
            }

            var result = Enumerable.Empty<DynamicReference>();

            if (expression.LeftOperand is DynamicReference)
            {
                result = result.Concat(new[] {(DynamicReference) expression.LeftOperand});
            }
            if (expression.RightOperand is DynamicReference)
            {
                result = result.Concat(new[] { (DynamicReference)expression.RightOperand });
            }

            return result;
        }
    }
}
