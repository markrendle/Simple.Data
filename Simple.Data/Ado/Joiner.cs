using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Simple.Data.Schema;

namespace Simple.Data.Ado
{
    class Joiner
    {
        private readonly JoinType _joinType;
        private readonly DatabaseSchema _schema;
        private readonly ConcurrentDictionary<string, string> _done = new ConcurrentDictionary<string, string>();

        public Joiner(JoinType joinType, DatabaseSchema schema)
        {
            if (schema == null) throw new ArgumentNullException("schema");
            _joinType = joinType;
            _schema = schema;
        }

        public string GetJoinClauses(string mainTableName, SimpleExpression expression)
        {
            _done.AddOrUpdate(mainTableName, string.Empty, (s, o) => string.Empty);
            foreach (var tablePair in GetTablePairs(expression))
            {
                AddJoin(tablePair.Item1, tablePair.Item2);
            }
            return string.Join(" ", _done.Values.Where(s => !string.IsNullOrWhiteSpace(s)));
        }

        private void AddJoin(string table1Name, string table2Name)
        {
            var table1 = _schema.FindTable(table1Name);
            var table2 = _schema.FindTable(table2Name);

            var foreignKey =
                table2.ForeignKeys.SingleOrDefault(fk => fk.MasterTable == table1.ActualName)
                ??
                table1.ForeignKeys.SingleOrDefault(fk => fk.MasterTable == table2.ActualName);

            if (foreignKey == null) throw new SchemaResolutionException(
                string.Format("Could not join '{0}' and '{1}'", table1.ActualName, table2.ActualName));

            _done.GetOrAdd(table2Name, s => MakeJoinText(foreignKey));
        }

        private string MakeJoinText(ForeignKey foreignKey)
        {
            var builder = new StringBuilder(JoinKeyword);
            builder.AppendFormat(" JOIN {2} ON ({0}.{1} = {2}.{3}", foreignKey.MasterTable, foreignKey.UniqueColumns[0],
                                 foreignKey.DetailTable, foreignKey.Columns[0]);

            for (int i = 1; i < foreignKey.Columns.Length; i++)
            {
                builder.AppendFormat(" AND {0}.{1} = {2}.{3}", foreignKey.MasterTable, foreignKey.UniqueColumns[i],
                                 foreignKey.DetailTable, foreignKey.Columns[i]);
            }
            builder.Append(")");
            return builder.ToString();
        }

        private string JoinKeyword
        {
            get { return _joinType == JoinType.Inner ? string.Empty : "LEFT"; }
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
