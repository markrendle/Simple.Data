using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using Simple.Data.Ado;
using System.IO;

namespace Simple.Data
{
    class ProviderHelper
    {
        private static readonly ConcurrentDictionary<string, IConnectionProvider> Cache = new ConcurrentDictionary<string,IConnectionProvider>();
        private static readonly Lazy<CompositionContainer> Composer = new Lazy<CompositionContainer>(CreateContainer);

        public static IConnectionProvider GetProviderByConnectionString(string connectionString)
        {
            return new SqlProvider(connectionString);
        }

        public static IConnectionProvider GetProviderByFilename(string filename)
        {
            return Cache.GetOrAdd(filename, LoadProvider);
        }

        private static IConnectionProvider LoadProvider(string filename)
        {
            var extension = (Path.GetExtension(filename) ?? "").Substring(1).ToLower();
            var export = Composer.Value.GetExport<IConnectionProvider>(extension);

            if (export == null) throw new ArgumentException("Unrecognised file.");

            var provider = export.Value;
            provider.SetConnectionString(string.Format("data source={0}", filename));
            return provider;
        }

        private static CompositionContainer CreateContainer()
        {
            var path = Assembly.GetExecutingAssembly().CodeBase.Replace("file:///", "");
            path = Path.GetDirectoryName(path);
            var catalog = new DirectoryCatalog(path, "Simple.Data.*.dll");
            return new CompositionContainer(catalog);
        }
    }
}
