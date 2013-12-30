using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Configuration;
using System.Data.OleDb;
using System.Linq;
using System.Reflection;
using System.IO;
using System.Text.RegularExpressions;

namespace Simple.Data.Ado
{
    using Schema;

    public class ProviderHelper
    {
        private readonly ConcurrentDictionary<ConnectionToken, IConnectionProvider> _connectionProviderCache = new ConcurrentDictionary<ConnectionToken,IConnectionProvider>();
        private readonly ConcurrentDictionary<Type, object> _customProviderCache = new ConcurrentDictionary<Type, object>();

        public IConnectionProvider GetProviderByConnectionString(string connectionString)
        {
            var token = new ConnectionToken(connectionString, string.Empty);
            return _connectionProviderCache.GetOrAdd(token, LoadProviderByConnectionString);
        }

        public IConnectionProvider GetProviderByFilename(string filename)
        {
            var token = new ConnectionToken(filename, "System.Data.SqlCeClient");
            return _connectionProviderCache.GetOrAdd(token, LoadProviderByFilename);
        }

        private IConnectionProvider LoadProviderByConnectionString(ConnectionToken token)
        {
            var dataSource = GetDataSourceName(token.ConnectionString);
            if (dataSource.EndsWith("sdf", StringComparison.CurrentCultureIgnoreCase) && File.Exists(dataSource))
            {
                return GetProviderByFilename(dataSource);
            }
            
            var provider = ComposeProvider();
            provider.SetConnectionString(token.ConnectionString);
            return provider;
        }
            
        internal static string GetDataSourceName(string connectionString)
        {
            var match = Regex.Match(connectionString, @"data source=(.*?)(;|\z)");
            if (match != null && match.Groups.Count > 1)
            {
                return match.Groups[1].Value;
            }
            return string.Empty;
        }

        private static IConnectionProvider LoadProviderByFilename(ConnectionToken token)
        {
            string extension = GetFileExtension(token.ConnectionString);

            var provider = ComposeProvider(extension);

            provider.SetConnectionString(string.Format("data source={0}", token.ConnectionString));
            return provider;
        }

        private static string GetFileExtension(string filename)
        {
            var extension = Path.GetExtension(filename);

            if (extension == null) throw new ArgumentException(string.Format("Unrecognised file name '{0}': no extension.", filename), "filename");
            return extension.TrimStart('.').ToLower();
        }

        private static IConnectionProvider ComposeProvider()
        {
            return Composer.Default.Compose<IConnectionProvider>();
        }

        private static IConnectionProvider ComposeProvider(string extension)
        {
            return Composer.Default.Compose<IConnectionProvider>(extension);
        }

        public IConnectionProvider GetProviderByConnectionName(string connectionName, string schemaName = null)
        {
            var connectionSettings = ConfigurationManager.ConnectionStrings[connectionName];
            if (connectionSettings == null)
            {
                throw new ArgumentOutOfRangeException("connectionName");
            }

            return GetProviderByConnectionString(connectionSettings.ConnectionString, connectionSettings.ProviderName, schemaName);
        }

        public IConnectionProvider GetProviderByConnectionString(string connectionString, string providerName, string schemaName = null)
        {
            return _connectionProviderCache.GetOrAdd(new ConnectionToken(connectionString, providerName, schemaName),
                                                     LoadProviderByConnectionToken);
        }

        private static IConnectionProvider LoadProviderByConnectionToken(ConnectionToken token)
        {
            IConnectionProvider provider;

            if (TryLoadAssemblyUsingAttribute(token.ConnectionString, token.ProviderName, out provider))
            {
                return provider;
            }

            provider = ComposeProvider(token.ProviderName);
            if (provider == null)
            {
                throw new InvalidOperationException(string.Format("Provider '{0}' could not be resolved.", token.ProviderName));
            }

            provider.SetConnectionString(token.ConnectionString);

            var schemaConnectionProvider = provider as ISchemaConnectionProvider;
            if(schemaConnectionProvider != null)
                schemaConnectionProvider.SetSchema(token.SchemaName);

            return provider;
            
        }

        public T GetCustomProvider<T>(IConnectionProvider connectionProvider)
        {
            return (T)_customProviderCache.GetOrAdd(typeof (T), t => GetCustomProviderExport<T>(connectionProvider.GetType().Assembly) ??
                                                                     GetCustomProviderServiceProvider(connectionProvider as IServiceProvider, t));
        }

        private static Object GetCustomProviderExport<T>(Assembly assembly)
        {
            using (var assemblyCatalog = new AssemblyCatalog(assembly))
            {
                using (var container = new CompositionContainer(assemblyCatalog))
                {
                    return container.GetExportedValueOrDefault<T>();
                }
            }
        }

        private static Object GetCustomProviderServiceProvider(IServiceProvider serviceProvider, Type type)
        {
            if (serviceProvider != null)
            {
                return serviceProvider.GetService(type);
            }
            return null;
        }

        public T GetCustomProvider<T>(ISchemaProvider schemaProvider)
        {
            return (T)_customProviderCache.GetOrAdd(typeof(T), t => GetCustomProviderExport<T>(schemaProvider));
        }

        private static T GetCustomProviderExport<T>(ISchemaProvider schemaProvider)
        {
            using (var assemblyCatalog = new AssemblyCatalog(schemaProvider.GetType().Assembly))
            {
                using (var container = new CompositionContainer(assemblyCatalog))
                {
                    return container.GetExportedValueOrDefault<T>();
                }
            }
        }

        internal static bool TryLoadAssemblyUsingAttribute(string connectionString, string providerName, out IConnectionProvider connectionProvider)
        {
            var attributes = LoadAssemblyAttributes();
            if (attributes.Count == 0)
            {
                connectionProvider = null;
                return false;
            }
            if (!string.IsNullOrWhiteSpace(providerName))
            {
                attributes = attributes.Where(a => a.IsForProviderName(providerName)).ToList();
            }
            if (attributes.Count == 0)
            {
                connectionProvider = null;
                return false;
            }

            return LoadUsingAssemblyAttribute(connectionString, attributes, out connectionProvider);
        }

        private static bool LoadUsingAssemblyAttribute(string connectionString, ICollection<ProviderAssemblyAttributeBase> attributes,
                                                       out IConnectionProvider connectionProvider)
        {
            if (attributes.Count == 0)
            {
                {
                    connectionProvider = null;
                    return true;
                }
            }

            foreach (var attribute in attributes)
            {
                Exception exception;
                if (attribute.TryGetProvider(connectionString, out connectionProvider, out exception))
                {
                    return true;
                }
            }
            connectionProvider = null;
            return false;
        }

        private static List<ProviderAssemblyAttributeBase> LoadAssemblyAttributes()
        {
            var attributes = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.GetFullName().StartsWith("Simple.Data.", StringComparison.OrdinalIgnoreCase))
                .SelectMany(ProviderAssemblyAttributeBase.Get)
                .ToList();

            if (attributes.Count == 0)
            {
                foreach (var file in Directory.EnumerateFiles(Composer.GetSimpleDataAssemblyPath(), "Simple.Data.*.dll"))
                {
                    Assembly assembly;
                    if (Composer.TryLoadAssembly(file, out assembly))
                    {
                        if (ProviderAssemblyAttributeBase.Get(assembly).Any())
                        {
                            assembly = Assembly.LoadFrom(file);
                            attributes.AddRange(ProviderAssemblyAttributeBase.Get(assembly));
                        }
                    }
                }
            }
            return attributes;
        }

        private class ConnectionToken : IEquatable<ConnectionToken>
        {
            /// <summary>
            /// Indicates whether the current object is equal to another object of the same type.
            /// </summary>
            /// <returns>
            /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
            /// </returns>
            /// <param name="other">An object to compare with this object.</param>
            public bool Equals(ConnectionToken other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Equals(other.ConnectionString, ConnectionString) && Equals(other.ProviderName, ProviderName);
            }

            /// <summary>
            /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
            /// </summary>
            /// <returns>
            /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
            /// </returns>
            /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>. </param><filterpriority>2</filterpriority>
            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != typeof (ConnectionToken)) return false;
                return Equals((ConnectionToken) obj);
            }

            /// <summary>
            /// Serves as a hash function for a particular type. 
            /// </summary>
            /// <returns>
            /// A hash code for the current <see cref="T:System.Object"/>.
            /// </returns>
            /// <filterpriority>2</filterpriority>
            public override int GetHashCode()
            {
                unchecked
                {
                    return (ConnectionString.GetHashCode()*397) ^ (ProviderName.GetHashCode() * 397) ^ SchemaName.GetHashCode();
                }
            }

            public static bool operator ==(ConnectionToken left, ConnectionToken right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(ConnectionToken left, ConnectionToken right)
            {
                return !Equals(left, right);
            }

            private readonly string _connectionString;
            private readonly string _providerName;
            private readonly string _schemaName;

            public ConnectionToken(string connectionString, string providerName, string schemaName = null)
            {
                if (connectionString == null) throw new ArgumentNullException("connectionString");
                _connectionString = connectionString;
                _providerName = providerName ?? string.Empty;
                _schemaName = schemaName ?? string.Empty;
            }

            public string ConnectionString
            {
                get { return _connectionString; }
            }

            public string ProviderName
            {
                get { return _providerName; }
            }

            public string SchemaName
            {
                get { return _schemaName; }
            }
        }
    }
}
