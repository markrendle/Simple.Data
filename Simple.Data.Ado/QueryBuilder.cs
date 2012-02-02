using System;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using Simple.Data.Ado.Schema;

namespace Simple.Data.Ado
{
    using Extensions;

    public class QueryBuilder
    {
        private readonly SimpleReferenceFormatter _simpleReferenceFormatter;
        private readonly AdoAdapter _adoAdapter;
        private readonly int _bulkIndex;
        private readonly DatabaseSchema _schema;

        private ObjectName _tableName;
        private Table _table;
        private SimpleQuery _query;
        private SimpleExpression _whereCriteria;
        private SimpleExpression _havingCriteria;
        private IList<SimpleReference> _columns;
        private readonly CommandBuilder _commandBuilder;
        private List<SimpleQueryClauseBase> _unhandledClauses;

        public QueryBuilder(AdoAdapter adoAdapter) : this(adoAdapter, -1)
        {
        }

        public QueryBuilder(AdoAdapter adoAdapter, int bulkIndex)
        {
            _adoAdapter = adoAdapter;
            _bulkIndex = bulkIndex;
            _schema = _adoAdapter.GetSchema();
            _commandBuilder = new CommandBuilder(_schema, _bulkIndex);
            _simpleReferenceFormatter = new SimpleReferenceFormatter(_schema, _commandBuilder);
        }

        public ICommandBuilder Build(SimpleQuery query, out IEnumerable<SimpleQueryClauseBase> unhandledClauses)
        {
            _unhandledClauses = new List<SimpleQueryClauseBase>();
            SetQueryContext(query);

            HandleJoins();
            HandleQueryCriteria();
            HandleGrouping();
            HandleHavingCriteria();
            HandleOrderBy();

            unhandledClauses = _unhandledClauses;
            return _commandBuilder;
        }

        private void SetQueryContext(SimpleQuery query)
        {
            _query = query;
            _tableName = _schema.BuildObjectName(query.TableName);
            _table = _schema.FindTable(_tableName);
            var selectClause = _query.Clauses.OfType<SelectClause>().SingleOrDefault();
            if (selectClause != null)
            {
                if (selectClause.Columns.OfType<AllColumnsSpecialReference>().Any())
                {
                    _columns = ExpandAllColumnsReferences(selectClause.Columns).ToArray();
                }
                else
                {
                    _columns = selectClause.Columns.ToArray();
                }
            }
            else
            {
                _columns = _table.Columns.Select(c => ObjectReference.FromStrings(_table.Schema, _table.ActualName, c.ActualName)).ToArray();
            }

            HandleWithClauses();

            _whereCriteria = _query.Clauses.OfType<WhereClause>().Aggregate(SimpleExpression.Empty,
                                                                            (seed, where) => seed && where.Criteria);
            _havingCriteria = _query.Clauses.OfType<HavingClause>().Aggregate(SimpleExpression.Empty,
                                                                              (seed, having) => seed && having.Criteria);

            _commandBuilder.SetText(GetSelectClause(_tableName));
        }

        private IEnumerable<SimpleReference> ExpandAllColumnsReferences(IEnumerable<SimpleReference> columns)
        {
            foreach (var column in columns)
            {
                var allColumns = column as AllColumnsSpecialReference;
                if (ReferenceEquals(allColumns, null)) yield return column;
                else
                {
                    foreach (var allColumn in _schema.FindTable(allColumns.Table.GetName()).Columns)
                    {
                        yield return new ObjectReference(allColumn.ActualName, allColumns.Table);
                    }
                }
            }
        }

        private void HandleWithClauses()
        {
            var withClauses = _query.Clauses.OfType<WithClause>().ToList();
            var relationTypeDict = new Dictionary<ObjectReference, RelationType>();
            if (withClauses.Count > 0)
            {
                foreach (var withClause in withClauses)
                {
                    if (withClause.ObjectReference.GetOwner().IsNull())
                    {
                        var joinClause =
                            _query.Clauses.OfType<JoinClause>().FirstOrDefault(j => j.Table.GetAliasOrName() == withClause.ObjectReference.GetAliasOrName());
                        if (joinClause != null)
                        {
                            _columns =
                                _columns.Concat(
                                    _schema.FindTable(joinClause.Table.GetName()).Columns.Select(
                                        c => new ObjectReference(c.ActualName, joinClause.Table)))
                                    .ToArray();
                            relationTypeDict[joinClause.Table] = withClause.Type == WithType.One
                                                                     ? RelationType.ManyToOne
                                                                     : RelationType.OneToMany;
                        }
                    }
                    else
                    {
                        relationTypeDict[withClause.ObjectReference] = RelationType.None;
                        _columns =
                            _columns.Concat(
                                _schema.FindTable(withClause.ObjectReference.GetName()).Columns.Select(
                                    c => new ObjectReference(c.ActualName, withClause.ObjectReference)))
                                .ToArray();
                    }
                }
                _columns =
                    _columns.OfType<ObjectReference>()
                        .Select(c => IsCoreTable(c.GetOwner()) ? c : AddWithAlias(c, relationTypeDict[c.GetOwner()]))
                        .ToArray();
            }
        }

        private bool IsCoreTable(ObjectReference tableReference)
        {
            if (ReferenceEquals(tableReference, null)) throw new ArgumentNullException("tableReference");
            if (!string.IsNullOrWhiteSpace(tableReference.GetAlias())) return false;
            return _schema.FindTable(tableReference.GetName()) == _table;
        }

        private ObjectReference AddWithAlias(ObjectReference c, RelationType relationType = RelationType.None)
        {
            if (relationType == RelationType.None)
                relationType = _schema.GetRelationType(c.GetOwner().GetOwner().GetName(), c.GetOwner().GetName());
            if (relationType == RelationType.None) throw new InvalidOperationException("No Join found");
            return c.As(string.Format("__with{0}__{1}__{2}",
                               relationType == RelationType.OneToMany
                                   ? "n"
                                   : "1", c.GetOwner().GetAliasOrName(), c.GetName()));
        }

        private void HandleJoins()
        {
            if (_whereCriteria == SimpleExpression.Empty && _havingCriteria == SimpleExpression.Empty
                && (!_query.Clauses.OfType<JoinClause>().Any())
                && (_columns.All(r => (r is CountSpecialReference)))) return;

            var joiner = new Joiner(JoinType.Inner, _schema);

            string dottedTables = RemoveSchemaFromQueryTableName();

            var fromTable = dottedTables.Contains('.')
                                ? joiner.GetJoinClauses(_tableName, dottedTables.Split('.').Reverse())
                                : Enumerable.Empty<string>();

            var joinClauses = _query.Clauses.OfType<JoinClause>().ToArray();
            var fromJoins = joiner.GetJoinClauses(joinClauses, _commandBuilder);

            var fromCriteria = joiner.GetJoinClauses(_tableName, _whereCriteria);

            var fromHavingCriteria = joiner.GetJoinClauses(_tableName, _havingCriteria);

            var fromColumnList = _columns.Any(r => !(r is SpecialReference))
                                     ? joiner.GetJoinClauses(_tableName, GetObjectReferences(_columns).Where(o => !joinClauses.Any(j => o.GetOwner().Equals(j.Table))), JoinType.Outer)
                                     : Enumerable.Empty<string>();

            var joins = string.Join(" ", fromTable.Concat(fromJoins)
                                             .Concat(fromCriteria)
                                             .Concat(fromHavingCriteria)
                                             .Concat(fromColumnList)
                                             .Distinct());

            if (!string.IsNullOrWhiteSpace(joins))
            {
                _commandBuilder.Append(" " + joins);
            }
        }

        private IEnumerable<ObjectReference> GetObjectReferences(IEnumerable<SimpleReference> source)
        {
            var list = source.ToList();
            foreach (var objectReference in list.OfType<ObjectReference>())
            {
                yield return objectReference;
            }

            foreach (var objectReference in list.OfType<FunctionReference>().Select(fr => fr.Argument).OfType<ObjectReference>())
            {
                yield return objectReference;
            }
        }

        private string RemoveSchemaFromQueryTableName()
        {
            return _query.TableName.StartsWith(_table.Schema + '.', StringComparison.InvariantCultureIgnoreCase)
                       ? _query.TableName.Substring(_query.TableName.IndexOf('.') + 1)
                       : _query.TableName;
        }

        private void HandleQueryCriteria()
        {
            if (_whereCriteria == SimpleExpression.Empty) return;
            _commandBuilder.Append(" WHERE " + new ExpressionFormatter(_commandBuilder, _schema).Format(_whereCriteria));
        }

        private void HandleHavingCriteria()
        {
            if (_havingCriteria == SimpleExpression.Empty) return;
            _commandBuilder.Append(" HAVING " + new ExpressionFormatter(_commandBuilder, _schema).Format(_havingCriteria));
        }

        private void HandleGrouping()
        {
            if (_havingCriteria == SimpleExpression.Empty && !_columns.OfType<FunctionReference>().Any(fr => fr.IsAggregate)) return;

            var groupColumns =
                GetColumnsToSelect(_table).Where(c => (!(c is FunctionReference)) || !((FunctionReference) c).IsAggregate).ToList();

            if (groupColumns.Count == 0) return;

            _commandBuilder.Append(" GROUP BY " + string.Join(",", groupColumns.Select(_simpleReferenceFormatter.FormatColumnClauseWithoutAlias)));
        }

        private void HandleOrderBy()
        {
            if (!_query.Clauses.OfType<OrderByClause>().Any()) return;

            var orderNames = _query.Clauses.OfType<OrderByClause>().Select(ToOrderByDirective);
            _commandBuilder.Append(" ORDER BY " + string.Join(", ", orderNames));
        }

        private string ToOrderByDirective(OrderByClause item)
        {
            string name;
            if (_columns.Any(r => (!string.IsNullOrWhiteSpace(r.GetAlias())) && r.GetAlias().Equals(item.Reference.GetName())))
            {
                name = item.Reference.GetName();
            }
            else
            {
                name = _table.FindColumn(item.Reference.GetName()).QualifiedName;
            }

            var direction = item.Direction == OrderByDirection.Descending ? " DESC" : string.Empty;
            return name + direction;
        }

        private string GetSelectClause(ObjectName tableName)
        {
            var table = _schema.FindTable(tableName);
            string template = _query.Clauses.OfType<DistinctClause>().Any()
                                  ? "select distinct {0} from {1}"
                                  : "select {0} from {1}";
            return string.Format(template,
                GetColumnsClause(table),
                table.QualifiedName);
        }

        private string GetColumnsClause(Table table)
        {
            if (_columns != null && _columns.Count == 1 && _columns[0] is SpecialReference)
            {
                return FormatSpecialReference((SpecialReference) _columns[0]);
            }

            return string.Join(",", GetColumnsToSelect(table).Select(_simpleReferenceFormatter.FormatColumnClause));
        }

        private static string FormatSpecialReference(SpecialReference reference)
        {
            if (reference.GetType() == typeof(CountSpecialReference)) return "COUNT(*)";
            if (reference.GetType() == typeof(ExistsSpecialReference)) return "DISTINCT 1";
            throw new InvalidOperationException("SpecialReference type not recognised.");
        }

        private IEnumerable<SimpleReference> GetColumnsToSelect(Table table)
        {
            if (_columns != null && _columns.Count > 0)
            {
                return _columns;
            }
            else
            {
                return table.Columns.Select(c => ObjectReference.FromStrings(table.Schema, table.ActualName, c.ActualName));
            }
        }

        private string FormatGroupByColumnClause(SimpleReference reference)
        {
            var objectReference = reference as ObjectReference;
            if (!ReferenceEquals(objectReference, null))
            {
                var table = _schema.FindTable(objectReference.GetOwner().GetName());
                var column = table.FindColumn(objectReference.GetName());
                return string.Format("{0}.{1}", table.QualifiedName, column.QuotedName);
            }

            var functionReference = reference as FunctionReference;
            if (!ReferenceEquals(functionReference, null))
            {
                return FormatGroupByColumnClause(functionReference.Argument);
            }

            throw new InvalidOperationException("SimpleReference type not supported.");
        }
    }
}