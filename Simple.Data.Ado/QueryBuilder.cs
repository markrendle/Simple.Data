using System;
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