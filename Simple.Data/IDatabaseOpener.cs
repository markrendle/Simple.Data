namespace Shitty.Data
{
    public interface IDatabaseOpener
    {
        dynamic OpenDefault(string schemaName = null);
        dynamic OpenFile(string filename);
        dynamic OpenConnection(string connectionString);
        dynamic OpenConnection(string connectionString, string providerName);
        dynamic Open(string adapterName, object settings);
        dynamic OpenNamedConnection(string connectionName);
        dynamic OpenNamedConnection(string connectionName, string schemaName);
        void ClearAdapterCache();
        dynamic OpenConnection(string connectionString, string providerName, string schemaName);
    }
}