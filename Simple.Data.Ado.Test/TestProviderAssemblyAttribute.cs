using System;

namespace Shitty.Data.Ado.Test
{
    public class TestProviderAssemblyAttribute : ProviderAssemblyAttributeBase
    {
        public TestProviderAssemblyAttribute()
            : base("Test")
        {
        }

        public override bool TryGetProvider(string connectionString, out IConnectionProvider provider, out Exception exception)
        {
            if (connectionString.Equals("Test"))
            {
                provider = new StubConnectionProvider();
                exception = null;
                return true;
            }
            provider = null;
            exception = null;
            return false;
        }
    }
}