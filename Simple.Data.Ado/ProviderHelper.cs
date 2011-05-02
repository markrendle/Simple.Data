using System;
using System.Collections.Concurrent;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Configuration;
using System.Data.OleDb;
using System.Reflection;
using System.IO;
using System.Text.RegularExpressions;

namespace Simple.Data.Ado
{
    public class ProviderHelper
	{
        private readonly ConcurrentDictionary<string, IConnectionProvider> _connectionProviderCache = new ConcurrentDictionary<string,IConnectionProvider>();
        private readonly ConcurrentDictionary<Type, object> _customProviderCache = new ConcurrentDictionary<Type, object>();

        public IConnectionProvider GetProviderByConnectionString(string connectionString)
        {
            return _connectionProviderCache.GetOrAdd(connectionString, LoadProviderByConnectionString);
        }

        public IConnectionProvider GetProviderByFilename(string filename)
        {
            return _connectionProviderCache.GetOrAdd(filename, LoadProviderByFilename);
        }

        private IConnectionProvider LoadProviderByConnectionString(string connectionString)
        {
			var dataSource = GetDataSourceName(connectionString);
            if (dataSource.EndsWith("sdf", StringComparison.CurrentCultureIgnoreCase) && File.Exists(dataSource))
            {
                return GetProviderByFilename(dataSource);
            }
            
            var provider = ComposeProvider("sql");

            provider.SetConnectionString(connectionString);
            return provider;
        }
			
		internal static string GetDataSourceName(string connectionString)
		{
			var match = Regex.Match(connectionString, "data source=(.*?);");
			if (match != null)
			{
				return match.Groups[1].Value;
			}
			return string.Empty;
		}

        private static IConnectionProvider LoadProviderByFilename(string filename)
        {
            string extension = GetFileExtension(filename);

            var provider = ComposeProvider(extension);

            provider.SetConnectionString(string.Format("data source={0}", filename));
            return provider;
        }

        private static string GetFileExtension(string filename)
        {
            var extension = Path.GetExtension(filename);

            if (extension == null) throw new ArgumentException("Unrecognised file.");
            return extension.TrimStart('.').ToLower();
        }

        private static IConnectionProvider ComposeProvider(string extension)
        {
            return MefHelper.Compose<IConnectionProvider>(extension);
        }

        public IConnectionProvider GetProviderByConnectionName(string connectionName)
        {
            var connectionSettings = ConfigurationManager.ConnectionStrings[connectionName];
            if (connectionSettings == null)
            {
                throw new ArgumentOutOfRangeException("connectionName");
            }

            if (connectionSettings.ProviderName == "System.Data.SqlClient")
            {
                return GetProviderByConnectionString(connectionSettings.ConnectionString);
            }

            var provider = ComposeProvider(connectionSettings.ProviderName);
            if (provider == null)
            {
                throw new InvalidOperationException("Provider could not be resolved.");
            }

            provider.SetConnectionString(connectionSettings.ConnectionString);
            return provider;
        }

        public T GetCustomProvider<T>(IConnectionProvider connectionProvider)
        {
            return (T)_customProviderCache.GetOrAdd(typeof (T), t => GetCustomProviderExport<T>(connectionProvider));
        }

        private static T GetCustomProviderExport<T>(IConnectionProvider connectionProvider)
        {
            using (var assemblyCatalog = new AssemblyCatalog(connectionProvider.GetType().Assembly))
            {
                using (var container = new CompositionContainer(assemblyCatalog))
                {
                    return container.GetExportedValueOrDefault<T>();
                }
            }
        }
    }
}
