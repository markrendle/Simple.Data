using System;
using System.Collections.Concurrent;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using System.IO;

namespace Simple.Data.Ado
{
    class ProviderHelper
    {
        private static readonly ConcurrentDictionary<string, IConnectionProvider> Cache = new ConcurrentDictionary<string,IConnectionProvider>();

        public static IConnectionProvider GetProviderByConnectionString(string connectionString)
        {
            return Cache.GetOrAdd(connectionString, LoadProviderByConnectionString);
        }

        public static IConnectionProvider GetProviderByFilename(string filename)
        {
            return Cache.GetOrAdd(filename, LoadProviderByFilename);
        }

        private static IConnectionProvider LoadProviderByConnectionString(string connectionString)
        {
            var provider = ComposeProvider("sql");

            provider.SetConnectionString(connectionString);
            return provider;
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
    }
}
