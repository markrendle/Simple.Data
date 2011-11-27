using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Simple.Data
{
    using System.Diagnostics;

    class MefHelper : Composer
    {
        public override T Compose<T>()
        {
            using (var container = CreateContainer())
            {
                var exports = container.GetExports<T>().ToList();
                if (exports.Count == 0) throw new SimpleDataException("No ADO Provider found.");
                if (exports.Count > 1) throw new SimpleDataException("Multiple ADO Providers found; specify provider name or remove unwanted assemblies.");
                return exports.Single().Value;
            }
        }

        public override T Compose<T>(string contractName)
        {
            try
            {
                using (var container = CreateContainer())
                {
                    var exports = container.GetExports<T>(contractName).ToList();
                    if (exports.Count == 0) throw new SimpleDataException("No ADO Provider found.");
                    if (exports.Count > 1) throw new SimpleDataException("Multiple ADO Providers found; specify provider name or remove unwanted assemblies.");
                    return exports.Single().Value;
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                Trace.WriteLine(ex.Message);
                throw;
            }
        }

        public static T GetAdjacentComponent<T>(Type knownSiblingType)
        {
            using (var assemblyCatalog = new AssemblyCatalog(knownSiblingType.Assembly))
            {
                using (var container = new CompositionContainer(assemblyCatalog))
                {
                    return container.GetExportedValueOrDefault<T>();
                }
            }
        }

        static string GetThisAssemblyPath()
        {
            var path = Assembly.GetExecutingAssembly().CodeBase.Replace("file:///", "").Replace("file://", "//");
            path = Path.GetDirectoryName(path);
            if (path == null) throw new ArgumentException("Unrecognised file.");
            if (!Path.IsPathRooted(path))
            {
                path = Path.DirectorySeparatorChar + path;
            }
            return path;
        }

        private static CompositionContainer CreateContainer()
        {
			var path = GetThisAssemblyPath ();

            var assemblyCatalog = new AssemblyCatalog(Assembly.GetExecutingAssembly());
			var aggregateCatalog = new AggregateCatalog(assemblyCatalog);
			foreach (string file in System.IO.Directory.GetFiles(path, "Simple.Data.*.dll"))
			{
				var catalog = new AssemblyCatalog(file);
				aggregateCatalog.Catalogs.Add(catalog);
			}
            return new CompositionContainer(aggregateCatalog);
        }
    }
}
