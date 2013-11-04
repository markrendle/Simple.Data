using System;
using System.Data.SqlServerCe;
using Simple.Data.Ado;

namespace Simple.Data.SqlCe40
{
    public class SqlCe40ProviderAttribute : ProviderAssemblyAttributeBase
    {
        public SqlCe40ProviderAttribute() : base("sdf", "System.Data.SqlServerCe", "System.Data.SqlServerCe.4.0")
        {
        }

        public override bool TryGetProvider(string connectionString, out IConnectionProvider provider, out Exception exception)
        {
            try
            {
                var _ = new SqlCeConnectionStringBuilder(connectionString);
                provider = new SqlCe40ConnectionProvider();
                provider.SetConnectionString(connectionString);
                exception = null;
                return true;
            }
            catch (Exception ex)
            {
                exception = ex;
                provider = null;
                return false;
            }
        }
    }
}