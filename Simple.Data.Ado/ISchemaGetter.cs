using System.Data;

namespace Simple.Data.Ado
{
    public interface ISchemaGetter
    {
        DataTable GetSchema(string collectionName, params string[] constraints);
    }
}