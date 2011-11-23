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

        private static CompositionContainer CreateContainer()
        {
            var path = Assembly.GetExecutingAssembly().CodeBase.Replace("file:///", "").Replace("file://","//");
            path = Path.GetDirectoryName(path);
            if (path == null) throw new ArgumentException("Unrecognised file.");

            var assemblyCatalog = new AssemblyCatalog(Assembly.GetExecutingAssembly());
            var folderCatalog = new DirectoryCatalog(path, "Simple.Data.*.dll");
            return new CompositionContainer(new AggregateCatalog(assemblyCatalog, folderCatalog));
        }
    }
}
