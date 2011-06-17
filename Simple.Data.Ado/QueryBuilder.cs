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
            HandleGrouping();
            HandleOrderBy();
            HandlePaging();

            return _commandBuilder;
        }

        private void SetQueryContext(SimpleQuery query)
        {
            _query = query;
            _tableName = ObjectName.Parse(query.TableName.Split('.').Last());
            _table = _schema.FindTable(_tableName);
            _commandBuilder = new CommandBuilder(GetSelectClause(_tableName), _schema.SchemaProvider);
        }

        private void HandleJoins()
        {
            var joiner = new Joiner(JoinType.Inner, _schema);

            string dottedTables = RemoveSchemaFromQueryTableName();

            var fromTable = dottedTables.Contains('.')
                                ? joiner.GetJoinClauses(_tableName, dottedTables.Split('.').Reverse())
                                : Enumerable.Empty<string>();

            var fromCriteria = _query.Criteria != null
                                   ? joiner.GetJoinClauses(_tableName, _query.Criteria)
                                   : Enumerable.Empty<string>();

            var fromColumnList = _query.Columns.Any(r => !(r is SpecialReference))
                                     ? joiner.GetJoinClauses(_tableName, _query.Columns.OfType<ObjectReference>())
                                     : Enumerable.Empty<string>();

            if (_query.Criteria == null
                && (_query.Columns.Where(r => !(r is CountSpecialReference)).Count() == 0)) return;

            var joins = string.Join(" ", fromTable.Concat(fromCriteria).Concat(fromColumnList).Distinct());

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
            if (_query.Criteria == null) return;
            _commandBuilder.Append(" WHERE " + new ExpressionFormatter(_commandBuilder, _schema).Format(_query.Criteria));
        }

        private void HandleGrouping()
        {
            if (!_query.Columns.OfType<FunctionReference>().Any(fr => fr.IsAggregate)) return;

            var groupColumns =
                _query.Columns.Where(c => (!(c is FunctionReference)) || !((FunctionReference) c).IsAggregate);

            if (!groupColumns.Any()) return;

            _commandBuilder.Append(" GROUP BY " + string.Join(",", groupColumns.Select(FormatGroupByColumnClause)));
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
                string.Join(",", GetColumnsToSelect(table).Select(FormatColumnClause));
        }

        private static string FormatSpecialReference(SpecialReference reference)
        {
            if (reference.GetType() == typeof(CountSpecialReference)) return "COUNT(*)";
            if (reference.GetType() == typeof(ExistsSpecialReference)) return "DISTINCT 1";
            throw new InvalidOperationException("SpecialReference type not recognised.");
        }

        private IEnumerable<SimpleReference> GetColumnsToSelect(Table table)
        {
            if (_query.Columns.Any())
            {
                return _query.Columns;
            }
            else
            {
                return table.Columns.Select(c => ObjectReference.FromStrings(table.ActualName, c.ActualName));
            }
        }

        private string FormatColumnClause(SimpleReference reference)
        {
            var formatted = TryFormatAsObjectReference(reference as ObjectReference)
                   ??
                   TryFormatAsFunctionReference(reference as FunctionReference);

            if (formatted != null) return formatted;

            throw new InvalidOperationException("SimpleReference type not supported.");
        }

        private string TryFormatAsFunctionReference(FunctionReference functionReference)
        {
            if (ReferenceEquals(functionReference, null)) return null;
            
            var sqlName = _functionNameConverter.ConvertToSqlName(functionReference.Name);
            return functionReference.Alias == null
                       ? string.Format("{0}({1})", sqlName,
                                       FormatColumnClause(functionReference.Argument))
                       : string.Format("{0}({1}) AS {2}", sqlName,
                                       FormatColumnClause(functionReference.Argument),
                                       _schema.QuoteObjectName(functionReference.Alias));
        }

        private string TryFormatAsObjectReference(ObjectReference objectReference)
        {
            if (ReferenceEquals(objectReference, null)) return null;

            var table = _schema.FindTable(objectReference.GetOwner().GetName());
            var column = table.FindColumn(objectReference.GetName());
            if (objectReference.Alias == null)
                return string.Format("{0}.{1}", table.QualifiedName, column.QuotedName);
            else
                return string.Format("{0}.{1} AS {2}", table.QualifiedName, column.QuotedName,
                                     _schema.QuoteObjectName(objectReference.Alias));
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

    class FunctionNameConverter : IFunctionNameConverter
    {
        public string ConvertToSqlName(string simpleFunctionName)
        {
            if (simpleFunctionName.Equals("length", StringComparison.InvariantCultureIgnoreCase))
            {
                return "len";
            }
            if (simpleFunctionName.Equals("average", StringComparison.InvariantCultureIgnoreCase))
            {
                return "avg";
            }
            return simpleFunctionName;
        }
    }

    public interface IFunctionNameConverter
    {
        string ConvertToSqlName(string simpleFunctionName);
    }
}