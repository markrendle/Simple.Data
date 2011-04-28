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

        public QueryBuilder(AdoAdapter adoAdapter)
        {
            _adoAdapter = adoAdapter;
            _schema = _adoAdapter.GetSchema();
        }

        public ICommandBuilder Build(SimpleQuery query)
        {
            var commandBuilder = new CommandBuilder(GetSelectClause(ObjectName.Parse(query.TableName)), _schema.SchemaProvider);

            if (query.Criteria != null)
            {
                commandBuilder.Append(" WHERE " + new ExpressionFormatter(commandBuilder, _schema).Format(query.Criteria));
            }

            if (query.Order != null)
            {
                var orderNames = query.Order.Select(q => q.Reference.GetName() + (q.Direction == OrderByDirection.Descending ? " DESC" : string.Empty));
                commandBuilder.Append(" ORDER BY " + string.Join(", ", orderNames));
            }

            if (query.SkipCount != null || query.TakeCount != null)
            {
                var queryPager = _adoAdapter.ProviderHelper.GetCustomProvider<IQueryPager>(_adoAdapter.ConnectionProvider);
                if (queryPager == null)
                {
                    throw new NotSupportedException("Paging is not supported by the current ADO provider.");
                }

                var skipTemplate = commandBuilder.AddParameter("skip", DbType.Int32, query.SkipCount ?? 0);
                var takeTemplate = commandBuilder.AddParameter("take", DbType.Int32, query.TakeCount ?? int.MaxValue - (query.SkipCount ?? 0));
                commandBuilder.SetText(queryPager.ApplyPaging(commandBuilder.Text, skipTemplate.Name, takeTemplate.Name));
            }

            return commandBuilder;
        }

        private string GetSelectClause(ObjectName tableName)
        {
            var table = _schema.FindTable(tableName);
            return string.Format("select {0} from {1}",
                string.Join(",", table.Columns.Select(c => c.QuotedName)),
                table.QualifiedName);
        }
    }
}