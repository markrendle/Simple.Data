namespace Simple.Data
{
    public interface IDatabaseOpener
    {
        dynamic OpenDefault();
        dynamic OpenFile(string filename);
        dynamic OpenConnection(string connectionString);
        dynamic OpenConnection(string connectionString, string providerName);
        dynamic Open(string adapterName, object settings);
        dynamic OpenNamedConnection(string connectionName);
        void ClearAdapterCache();
    }
}