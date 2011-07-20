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

        public IEnumerable<string> GetJoinClauses(ObjectName mainTableName, IEnumerable<string> tableList)
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

        public IEnumerable<string> GetJoinClauses(ObjectName mainTableName, IEnumerable<ObjectReference> references)
        {
            _done.AddOrUpdate(mainTableName, string.Empty, (s, o) => string.Empty);
            var tablePairs = GetTableNames(references, mainTableName.Schema).Distinct().ToList();
            foreach (var tablePair in tablePairs)
            {
                AddJoin(tablePair.Item1, tablePair.Item2);
            }
            return tablePairs.Select(tp => _done[tp.Item2]).Distinct();
        }

        public IEnumerable<string> GetJoinClauses(IEnumerable<JoinClause> joins, ICommandBuilder commandBuilder)
        {
            var expressionFormatter = new ExpressionFormatter(commandBuilder, _schema);
            foreach (var join in joins)
            {
                var builder = new StringBuilder(JoinKeyword);
                builder.AppendFormat(" JOIN {0}{1} ON ({2})",
                    _schema.FindTable(ObjectName.Parse(join.Table.ToString())).QualifiedName,
                    string.IsNullOrWhiteSpace(join.Table.Alias) ? string.Empty : " " + _schema.QuoteObjectName(join.Table.Alias),
                    expressionFormatter.Format(join.JoinExpression));
                yield return builder.ToString().Trim();
            }
        }

        private void AddJoin(ObjectName table1Name, ObjectName table2Name)
        {
            _done.GetOrAdd(table2Name, _ =>
                                           {
                                               var table1 = _schema.FindTable(table1Name);
                                               var table2 = _schema.FindTable(table2Name);

                                               var foreignKey =
                                                   table2.ForeignKeys.SingleOrDefault(fk => fk.MasterTable.Schema == table1.Schema && fk.MasterTable.Name == table1.ActualName)
                                                   ??
                                                   table1.ForeignKeys.SingleOrDefault(fk => fk.MasterTable.Schema == table2.Schema && fk.MasterTable.Name == table2.ActualName);

                                               if (foreignKey == null) throw new SchemaResolutionException(
                                                   string.Format("Could not join '{0}' and '{1}'", table1.ActualName, table2.ActualName));

                                               return MakeJoinText(table2, foreignKey);
                                           });
        }

        private string MakeJoinText(Table rightTable, ForeignKey foreignKey)
        {
            var builder = new StringBuilder(JoinKeyword);
            builder.AppendFormat(" JOIN {0} ON (", rightTable.QualifiedName);
            builder.Append(FormatJoinExpression(foreignKey, 0));

            for (int i = 1; i < foreignKey.Columns.Length; i++)
            {
                builder.Append(" AND ");
                builder.Append(FormatJoinExpression(foreignKey, i));
            }
            builder.Append(")");
            return builder.ToString();
        }

        private string FormatJoinExpression(ForeignKey foreignKey, int columnIndex)
        {
            return string.Format("{0}.{1} = {2}.{3}", _schema.QuoteObjectName(foreignKey.MasterTable), _schema.QuoteObjectName(foreignKey.UniqueColumns[columnIndex]),
                                 _schema.QuoteObjectName(foreignKey.DetailTable), _schema.QuoteObjectName(foreignKey.Columns[columnIndex]));
        }

        private string JoinKeyword
        {
            get { return _joinType == JoinType.Inner ? string.Empty : "LEFT"; }
        }

        private static IEnumerable<Tuple<ObjectName, ObjectName>> GetTableNames(IEnumerable<ObjectReference> references, string schema)
        {
            return references.SelectMany(r => DynamicReferenceToTuplePairs(r, schema))
                .TupleSelect((table1, table2) => Tuple.Create(new ObjectName(schema, table1), new ObjectName(schema, table2)))
                .Distinct();
        }

        private static IEnumerable<Tuple<ObjectName, ObjectName>> GetTableNames(SimpleExpression expression, string schema)
        {
            return expression == null ? Enumerable.Empty<Tuple<ObjectName, ObjectName>>() : GetTableNames(GetReferencesFromExpression(expression), schema);
        }

        private static IEnumerable<Tuple<string, string>> DynamicReferenceToTuplePairs(ObjectReference reference, string schema)
        {
            return reference.GetAllObjectNames()
                .SkipWhile(s => s.Equals(schema, StringComparison.OrdinalIgnoreCase))
                .SkipLast()
                .ToTuplePairs();
        }

        private static IEnumerable<ObjectReference> GetReferencesFromExpression(SimpleExpression expression)
        {
            if (expression.Type == SimpleExpressionType.And || expression.Type == SimpleExpressionType.Or)
            {
                return GetReferencesFromExpression((SimpleExpression)expression.LeftOperand)
                    .Concat(GetReferencesFromExpression((SimpleExpression)expression.LeftOperand));
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