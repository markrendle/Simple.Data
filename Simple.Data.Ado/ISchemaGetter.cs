using System.Data;

namespace Shitty.Data.Ado
{
    public interface ISchemaGetter
    {
        DataTable GetSchema(string collectionName, params string[] constraints);
    }
}