using System;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using Simple.Data.Ado.Schema;

namespace Simple.Data.Ado
{
    public class QueryBuilder
    {
        private readonly AdoAdapter _adoAdapter;
        private readonly DatabaseSchema _schema;

        private ObjectName _tableName;
        private Table _table;
        private SimpleQuery _query;
        private CommandBuilder _commandBuilder;

        public QueryBuilder(AdoAdapter adoAdapter)
        {
            _adoAdapter = adoAdapter;
            _schema = _adoAdapter.GetSchema();
        }

        public ICommandBuilder Build(SimpleQuery query)
        {
            SetQueryContext(query);

            HandleJoins();
            HandleQueryCriteria();
            HandleOrderBy();
            HandlePaging();

            return _commandBuilder;
        }

        private void SetQueryContext(SimpleQuery query)
        {
            _query = query;
            _tableName = ObjectName.Parse(query.TableName);
            _table = _schema.FindTable(_tableName);
            _commandBuilder = new CommandBuilder(GetSelectClause(_tableName), _schema.SchemaProvider);
        }

        private void HandleJoins()
        {
            if (_query.Criteria == null
                && (_query.Columns.Where(r => !(r is CountSpecialReference)).Count() == 0)) return;

            var joins = new Joiner(JoinType.Inner, _schema).GetJoinClauses(_tableName, _query.Criteria, _query.Columns.Where(r => !(r is CountSpecialReference)));
            if (!string.IsNullOrWhiteSpace(joins))
            {
                _commandBuilder.Append(" " + joins);
            }
        }

        private void HandleQueryCriteria()
        {
            if (_query.Criteria == null) return;
            _commandBuilder.Append(" WHERE " + new ExpressionFormatter(_commandBuilder, _schema).Format(_query.Criteria));
        }

        private void HandleOrderBy()
        {
            if (_query.Order == null) return;

            var orderNames = _query.Order.Select(ToOrderByDirective);
            _commandBuilder.Append(" ORDER BY " + string.Join(", ", orderNames));
        }

        private void HandlePaging()
        {
            if (_query.SkipCount != null || _query.TakeCount != null)
            {
                var queryPager = _adoAdapter.ProviderHelper.GetCustomProvider<IQueryPager>(_adoAdapter.ConnectionProvider);
                if (queryPager == null)
                {
                    throw new NotSupportedException("Paging is not supported by the current ADO provider.");
                }

                var skipTemplate = _commandBuilder.AddParameter("skip", DbType.Int32, _query.SkipCount ?? 0);
                var takeTemplate = _commandBuilder.AddParameter("take", DbType.Int32, _query.TakeCount ?? int.MaxValue - _query.SkipCount);
                _commandBuilder.SetText(queryPager.ApplyPaging(_commandBuilder.Text, skipTemplate.Name, takeTemplate.Name));
            }
        }

        private string ToOrderByDirective(SimpleOrderByItem item)
        {
            var col = _table.FindColumn(item.Reference.GetName());
            var direction = item.Direction == OrderByDirection.Descending ? " DESC" : string.Empty;
            return col.QuotedName + direction;
        }

        private string GetSelectClause(ObjectName tableName)
        {
            var table = _schema.FindTable(tableName);
            return string.Format("select {0} from {1}",
                GetColumnsClause(table),
                table.QualifiedName);
        }

        private string GetColumnsClause(Table table)
        {
            return _query.Columns.Count() == 1 && _query.Columns.Single() is SpecialReference
                ?
                FormatSpecialReference((SpecialReference)_query.Columns.Single())
                :
                string.Join(",", GetColumnsToSelect(table)
                .Select(c => FormatColumnClause(c.Item1, c.Item2, c.Item3)));
        }

        private static string FormatSpecialReference(SpecialReference reference)
        {
            if (reference.GetType() == typeof(CountSpecialReference)) return "COUNT(*)";
            if (reference.GetType() == typeof(ExistsSpecialReference)) return "DISTINCT 1";
            throw new InvalidOperationException("SpecialReference type not recognised.");
        }

        private IEnumerable<Tuple<Table,Column,string>> GetColumnsToSelect(Table table)
        {
            if (_query.Columns.Any())
            {
                return from c in _query.Columns
                       let t = _schema.FindTable(c.GetOwner().GetName())
                       select Tuple.Create(t, t.FindColumn(c.GetName()), c.Alias);
            }
            else
            {
                const string nullString = null;
                return table.Columns.Select(c => Tuple.Create(table, c, nullString));
            }
        }

        private string FormatColumnClause(Table table, Column column, string alias)
        {
            if (alias == null)
                return string.Format("{0}.{1}", table.QualifiedName, column.QuotedName);
            else
                return string.Format("{0}.{1} AS {2}", table.QualifiedName, column.QuotedName,
                                     _schema.QuoteObjectName(alias));
        }
    }
}