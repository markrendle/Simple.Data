using System.Collections.Generic;
using System.Data;

namespace Simple.Data.Ado.Schema
{
    public interface ISchemaProvider
    {
        IEnumerable<Table> GetTables();
        IEnumerable<Column> GetColumns(Table table);
        IEnumerable<Procedure> GetStoredProcedures();
        IEnumerable<Parameter> GetParameters(Procedure storedProcedure);
        Key GetPrimaryKey(Table table);
        IEnumerable<ForeignKey> GetForeignKeys(Table table);
        string QuoteObjectName(string unquotedName);
        string NameParameter(string baseName);
    }
}