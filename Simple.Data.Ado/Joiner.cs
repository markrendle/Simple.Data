using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Simple.Data.Ado.Schema;
using Simple.Data.Extensions;

namespace Simple.Data.Ado
{
    class Joiner
    {
        private readonly JoinType _joinType;
        private readonly DatabaseSchema _schema;
        private readonly ConcurrentDictionary<ObjectName, string> _done = new ConcurrentDictionary<ObjectName, string>();

        public Joiner(DatabaseSchema schema)
            : this(JoinType.Outer, schema)
        {
        }

        public Joiner(JoinType joinType, DatabaseSchema schema)
        {
            if (schema == null) throw new ArgumentNullException("schema");
            _joinType = joinType;
            _schema = schema;
        }

        public IEnumerable<string> GetJoinClauses(ObjectName mainTableName, IEnumerable<string> tableList, JoinType joinType = JoinType.Inner)
        {
            var tablePairs = tableList.Select(t => new ObjectName(mainTableName.Schema, t)).ToTuplePairs().ToList();
            foreach (var tablePair in tablePairs)
            {
                AddJoin(tablePair.Item1, tablePair.Item2);
            }
            return tablePairs.Select(tp => _done[tp.Item2]);
        }

        public IEnumerable<string> GetJoinClauses(ObjectName mainTableName, SimpleExpression expression)
        {
            if (expression == SimpleExpression.Empty) return Enumerable.Empty<string>();

            _done.AddOrUpdate(mainTableName, string.Empty, (s, o) => string.Empty);
            var tablePairs = GetTableNames(expression, mainTableName.Schema).Distinct().ToList();
            foreach (var tablePair in tablePairs)
            {
                AddJoin(tablePair.Item1, tablePair.Item2);
            }
            return tablePairs.Select(tp => _done[tp.Item2]).Distinct();
        }

        public IEnumerable<string> GetJoinClauses(ObjectName mainTableName, IEnumerable<ObjectReference> references, JoinType joinType = JoinType.Inner)
        {
            _done.AddOrUpdate(mainTableName, string.Empty, (s, o) => string.Empty);
            var tablePairs = GetTableNames(references, mainTableName.Schema).Distinct().ToList();
            foreach (var tablePair in tablePairs)
            {
                AddJoin(tablePair.Item1, tablePair.Item2, joinType);
            }
            return tablePairs.Select(tp => _done[tp.Item2]).Distinct();
        }

        public IEnumerable<string> GetJoinClauses(IEnumerable<JoinClause> joins, ICommandBuilder commandBuilder)
        {
            var expressionFormatter = new ExpressionFormatter(commandBuilder, _schema);
            foreach (var join in joins)
            {
                var builder = new StringBuilder(JoinTypeToKeyword(join.JoinType));
                var joinExpression = join.JoinExpression ?? InferJoinExpression(join.Table);
                if (!ReferenceEquals(joinExpression, null))
                {
                    builder.AppendFormat(" JOIN {0}{1} ON ({2})",
                                         _schema.FindTable(_schema.BuildObjectName(join.Table.ToString())).QualifiedName,
                                         string.IsNullOrWhiteSpace(join.Table.GetAlias())
                                             ? string.Empty
                                             : " " + _schema.QuoteObjectName(join.Table.GetAlias()),
                                         expressionFormatter.Format(joinExpression));
                    yield return builder.ToString().Trim();
                }
            }
        }

        private SimpleExpression InferJoinExpression(ObjectReference table)
        {
            if (table.GetOwner().IsNull()) return null;
            var table1 = _schema.FindTable(table.GetOwner().GetName());
            var table2 = _schema.FindTable(table.GetName());
            var foreignKey = GetForeignKey(table1, table2);
            return MakeJoinExpression(table, foreignKey);
        }

        private void AddJoin(ObjectName table1Name, ObjectName table2Name, JoinType joinType = JoinType.Inner)
        {
            _done.GetOrAdd(table2Name, _ =>
                                           {
                                               var table1 = _schema.FindTable(table1Name);
                                               var table2 = _schema.FindTable(table2Name);
                                               var foreignKey = GetForeignKey(table1, table2);
                                               return MakeJoinText(table2, table2Name.Alias, foreignKey, joinType);
                                           });
        }

        private static ForeignKey GetForeignKey(Table table1, Table table2)
        {
            var foreignKey =
                table2.ForeignKeys.SingleOrDefault(
                    fk => fk.MasterTable.Schema == table1.Schema && fk.MasterTable.Name == table1.ActualName)
                ??
                table1.ForeignKeys.SingleOrDefault(
                    fk => fk.MasterTable.Schema == table2.Schema && fk.MasterTable.Name == table2.ActualName);

            if (foreignKey == null)
                throw new SchemaResolutionException(
                    string.Format("Could not join '{0}' and '{1}'", table1.ActualName, table2.ActualName));
            return foreignKey;
        }

        private string MakeJoinText(Table rightTable, string alias, ForeignKey foreignKey, JoinType joinType)
        {
            var builder = new StringBuilder(JoinKeywordFor(joinType));
            builder.AppendFormat(" JOIN {0}", rightTable.QualifiedName);
            if (!string.IsNullOrWhiteSpace(alias)) builder.Append(" " + _schema.QuoteObjectName(alias));
            builder.Append(" ON (");
            builder.Append(FormatJoinExpression(foreignKey, 0, alias));

            for (int i = 1; i < foreignKey.Columns.Length; i++)
            {
                builder.Append(" AND ");
                builder.Append(FormatJoinExpression(foreignKey, i, alias));
            }
            builder.Append(")");
            return builder.ToString();
        }

        private SimpleExpression MakeJoinExpression(ObjectReference table, ForeignKey foreignKey)
        {
            var expression = CreateJoinExpression(table, foreignKey, 0);

            for (int i = 1; i < foreignKey.Columns.Length; i++)
            {
                expression = expression && CreateJoinExpression(table, foreignKey, i);
            }

            return expression;
        }

        private string JoinKeywordFor(JoinType joinType)
        {
            return joinType == JoinType.Inner ? string.Empty : "LEFT";
        }

        private SimpleExpression CreateJoinExpression(ObjectReference table, ForeignKey foreignKey, int columnIndex)
        {
            var masterObjectReference = ObjectReference.FromStrings(foreignKey.MasterTable.Name,
                                                                    foreignKey.UniqueColumns[columnIndex]);
            var detailObjectReference = ObjectReference.FromStrings(foreignKey.DetailTable.Name,
                                                                    foreignKey.Columns[columnIndex]);

            if (!string.IsNullOrWhiteSpace(table.GetAlias()))
            {
                if (detailObjectReference.GetOwner().GetName() == table.GetName())
                    detailObjectReference = new ObjectReference(detailObjectReference.GetName(), table);
                else if (masterObjectReference.GetOwner().GetName() == table.GetName())
                    masterObjectReference = new ObjectReference(masterObjectReference.GetName(), table);
            }

            return masterObjectReference == detailObjectReference;
        }

        private string FormatJoinExpression(ForeignKey foreignKey, int columnIndex, string alias)
        {
            var leftTable = string.IsNullOrWhiteSpace(alias)
                                ? _schema.QuoteObjectName(foreignKey.MasterTable)
                                : _schema.QuoteObjectName(alias);
            return string.Format("{0}.{1} = {2}.{3}", leftTable, _schema.QuoteObjectName(foreignKey.UniqueColumns[columnIndex]),
                                 _schema.QuoteObjectName(foreignKey.DetailTable), _schema.QuoteObjectName(foreignKey.Columns[columnIndex]));
        }

        private string JoinKeyword
        {
            get { return _joinType == JoinType.Inner ? string.Empty : "LEFT"; }
        }

        private string JoinTypeToKeyword(JoinType joinType)
        {
            return joinType == JoinType.Inner ? string.Empty : "LEFT";
        }

        private static IEnumerable<Tuple<ObjectName, ObjectName>> GetTableNames(IEnumerable<ObjectReference> references, string schema)
        {
            return references.SelectMany(r => DynamicReferenceToTuplePairs(r, schema))
                .TupleSelect((table1, table2) => Tuple.Create(new ObjectName(schema, table1.Item1, table1.Item2), new ObjectName(schema, table2.Item1, table2.Item2)))
                .Distinct();
        }

        private static IEnumerable<Tuple<ObjectName, ObjectName>> GetTableNames(SimpleExpression expression, string schema)
        {
            return expression == null ? Enumerable.Empty<Tuple<ObjectName, ObjectName>>() : GetTableNames(GetReferencesFromExpression(expression), schema);
        }

        private static IEnumerable<Tuple<Tuple<string, string>, Tuple<string, string>>> DynamicReferenceToTuplePairs(ObjectReference reference, string schema)
        {
            return reference.GetAllObjectNamesAndAliases()
                .SkipWhile(s => s.Item1.Equals(schema, StringComparison.OrdinalIgnoreCase))
                .SkipLast()
                .ToTuplePairs();
        }

        private static IEnumerable<ObjectReference> GetReferencesFromExpression(SimpleExpression expression)
        {
            if (expression.Type == SimpleExpressionType.And || expression.Type == SimpleExpressionType.Or)
            {
                return GetReferencesFromExpression((SimpleExpression)expression.LeftOperand)
                    .Concat(GetReferencesFromExpression((SimpleExpression)expression.RightOperand));
            }

            return Enumerable.Empty<ObjectReference>()
                .Concat(GetObjectReference(expression.LeftOperand))
                .Concat(GetObjectReference(expression.RightOperand));
        }

        private static IEnumerable<ObjectReference> GetObjectReference(object obj)
        {
            var objectReference = obj as ObjectReference;
            if (ReferenceEquals(objectReference, null))
            {
                var functionReference = obj as FunctionReference;
                if (!(ReferenceEquals(functionReference, null)))
                {
                    objectReference = functionReference.Argument as ObjectReference;
                }
            }

            if (!(ReferenceEquals(objectReference, null)))
            {
                yield return objectReference;
            }
        }
    }
}