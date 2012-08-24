using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Simple.Data.Ado;

namespace Simple.Data.SqlServer
{
    public class SqlServerProviderAttribute : ProviderAssemblyAttributeBase
    {
        public SqlServerProviderAttribute()
            : base("System.Data.SqlClient")
        {
        }

        public override bool TryGetProvider(string connectionString, out IConnectionProvider provider, out Exception exception)
        {
            try
            {
                var _ = new SqlConnectionStringBuilder(connectionString);
            }
            catch (KeyNotFoundException ex)
            {
                exception = ex;
                provider = null;
                return false;
            }
            catch (FormatException ex)
            {
                exception = ex;
                provider = null;
                return false;
            }
            catch (ArgumentException ex)
            {
                exception = ex;
                provider = null;
                return false;
            }

            provider = new SqlConnectionProvider(connectionString);
            exception = null;
            return true;
        }
    }
}