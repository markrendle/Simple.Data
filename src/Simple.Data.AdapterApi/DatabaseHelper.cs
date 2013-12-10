using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.AdapterApi
{
    /// <summary>
    /// Provides access to internal types and methods of the Simple.Data main assembly
    /// to be used by Adapter authors for extending the library.
    /// </summary>
    public static class DatabaseHelper
    {
        /// <summary>
        /// Creates a <see cref="Database"/> using the specified adapter.
        /// </summary>
        /// <param name="adapter">The adapter.</param>
        /// <returns>A new <see cref="Database"/> instance.</returns>
        public static Database Create(Adapter adapter)
        {
            return new Database(adapter);
        }
    }
}
