using System;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using Simple.Data.Ado.Schema;

namespace Simple.Data.Ado
{
    public class QueryBuilder
    {
        private readonly IFunctionNameConverter _functionNameConverter = new FunctionNameConverter();
        private SimpleReferenceFormatter _simpleReferenceFormatter;
        private readonly AdoAdapter _adoAdapter;
        private readonly int _bulkIndex;
        private readonly DatabaseSchema _schema;

        private ObjectName _tableName;
        private Table _table;
        private SimpleQuery _query;
        private SimpleExpression _whereCriteria;
        private SimpleExpression _havingCriteria;
        private SimpleReference[] _columns;
        private CommandBuilder _commandBuilder;
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
            HandlePaging();

            unhandledClauses = _unhandledClauses;
            return _commandBuilder;
        }

        private void SetQueryContext(SimpleQuery query)
        {
            _query = query;
            var selectClause = _query.Clauses.OfType<SelectClause>().SingleOrDefault();
            if (selectClause != null)
            {
                _columns = selectClause.Columns.ToArray();
            }
            else
            {
                _columns = new SimpleReference[0];
            }

            _whereCriteria = _query.Clauses.OfType<WhereClause>().Aggregate(SimpleExpression.Empty,
                                                                            (seed, where) => seed && where.Criteria);
            _havingCriteria = _query.Clauses.OfType<HavingClause>().Aggregate(SimpleExpression.Empty,
                                                                              (seed, having) => seed && having.Criteria);

            _tableName = ObjectName.Parse(query.TableName.Split('.').Last());
            _table = _schema.FindTable(_tableName);
            _commandBuilder.SetText(GetSelectClause(_tableName));
        }

        private void HandleJoins()
        {
            if (_whereCriteria == SimpleExpression.Empty && _havingCriteria == SimpleExpression.Empty
                && (!_query.Clauses.OfType<JoinClause>().Any())
                && (_columns.Where(r => !(r is CountSpecialReference)).Count() == 0)) return;

            var joiner = new Joiner(JoinType.Inner, _schema);

            string dottedTables = RemoveSchemaFromQueryTableName();

            var fromTable = dottedTables.Contains('.')
                                ? joiner.GetJoinClauses(_tableName, dottedTables.Split('.').Reverse())
                                : Enumerable.Empty<string>();

            var fromJoins = joiner.GetJoinClauses(_query.Clauses.OfType<JoinClause>(), _commandBuilder);

            var fromCriteria = joiner.GetJoinClauses(_tableName, _whereCriteria);

            var fromHavingCriteria = joiner.GetJoinClauses(_tableName, _havingCriteria);

            var fromColumnList = _columns.Any(r => !(r is SpecialReference))
                                     ? joiner.GetJoinClauses(_tableName, _columns.OfType<ObjectReference>())
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

        private string RemoveSchemaFromQueryTableName()
        {
            return _query.TableName.StartsWith(_table.Schema + '.')
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

            _commandBuilder.Append(" GROUP BY " + string.Join(",", groupColumns.Select(FormatGroupByColumnClause)));
        }

        private void HandleOrderBy()
        {
            if (!_query.Clauses.OfType<OrderByClause>().Any()) return;

            var orderNames = _query.Clauses.OfType<OrderByClause>().Select(ToOrderByDirective);
            _commandBuilder.Append(" ORDER BY " + string.Join(", ", orderNames));
        }

        private void HandlePaging()
        {
            const int maxInt = 2147483646;

            var skipClause = _query.Clauses.OfType<SkipClause>().FirstOrDefault() ?? new SkipClause(0);
            var takeClause = _query.Clauses.OfType<TakeClause>().FirstOrDefault() ?? new TakeClause(maxInt);
            if (skipClause.Count != 0 || takeClause.Count != maxInt)
            {
                var queryPager = _adoAdapter.ProviderHelper.GetCustomProvider<IQueryPager>(_adoAdapter.ConnectionProvider);
                if (queryPager == null)
                {
                    _unhandledClauses.AddRange(_query.OfType<SkipClause>());
                    _unhandledClauses.AddRange(_query.OfType<TakeClause>());
                }

                var skipTemplate = _commandBuilder.AddParameter("skip", DbType.Int32, skipClause.Count);
                var takeTemplate = _commandBuilder.AddParameter("take", DbType.Int32, takeClause.Count);
                _commandBuilder.SetText(queryPager.ApplyPaging(_commandBuilder.Text, skipTemplate.Name, takeTemplate.Name));
            }
        }

        private string ToOrderByDirective(OrderByClause item)
        {
            var col = _table.FindColumn(item.Reference.GetName());
            var direction = item.Direction == OrderByDirection.Descending ? " DESC" : string.Empty;
            return col.QuotedName + direction;
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
            if (_columns != null && _columns.Length == 1 && _columns[0] is SpecialReference)
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
            if (_columns != null && _columns.Length > 0)
            {
                return _columns;
            }
            else
            {
                return table.Columns.Select(c => ObjectReference.FromStrings(table.ActualName, c.ActualName));
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