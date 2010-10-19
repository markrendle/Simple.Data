using System.Data;

namespace Simple.Data.Ado
{
    public interface ISchemaProvider
    {
        DataTable GetSchema(string collectionName);
        DataTable GetSchema(string collectionName, params string[] restrictionValues);
    }
}