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
        private readonly ConcurrentDictionary<ConnectionToken, IConnectionProvider> _connectionProviderCache = new ConcurrentDictionary<ConnectionToken,IConnectionProvider>();
        private readonly ConcurrentDictionary<Type, object> _customProviderCache = new ConcurrentDictionary<Type, object>();

        public IConnectionProvider GetProviderByConnectionString(string connectionString)
        {
            var token = new ConnectionToken(connectionString, "System.Data.SqlClient");
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
            
            var provider = ComposeProvider("System.Data.SqlClient");

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

            return GetProviderByConnectionString(connectionSettings.ConnectionString, connectionSettings.ProviderName);
        }

        public IConnectionProvider GetProviderByConnectionString(string connectionString, string providerName)
        {
            return _connectionProviderCache.GetOrAdd(new ConnectionToken(connectionString, providerName),
                                                     LoadProviderByConnectionToken);
        }

        private static IConnectionProvider LoadProviderByConnectionToken(ConnectionToken token)
        {
            var provider = ComposeProvider(token.ProviderName);
            if (provider == null)
            {
                throw new InvalidOperationException("Provider could not be resolved.");
            }

            provider.SetConnectionString(token.ConnectionString);
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
                    return (ConnectionString.GetHashCode()*397) ^ ProviderName.GetHashCode();
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

            public ConnectionToken(string connectionString, string providerName)
            {
                if (connectionString == null) throw new ArgumentNullException("connectionString");
                _connectionString = connectionString;
                _providerName = providerName ?? string.Empty;
            }

            public string ConnectionString
            {
                get { return _connectionString; }
            }

            public string ProviderName
            {
                get { return _providerName; }
            }
        }
	}
}
