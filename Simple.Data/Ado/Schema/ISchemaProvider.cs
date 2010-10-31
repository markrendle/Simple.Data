using System.Collections.Generic;
using System.Data;

namespace Simple.Data.Ado.Schema
{
    public interface ISchemaProvider
    {
        IEnumerable<Table> GetTables();
        IEnumerable<Column> GetColumns(Table table);
        Key GetPrimaryKey(Table table);
        IEnumerable<ForeignKey> GetForeignKeys(Table table);
        string QuoteObjectName(string unquotedName);
    }
}