namespace Simple.Data.Ado
{
    public interface ISchemaConnectionProvider : IConnectionProvider
    {
        void SetSchema(string schema);
        string Schema { get; }
        string ConnectionProviderKey { get; }
    }
}
