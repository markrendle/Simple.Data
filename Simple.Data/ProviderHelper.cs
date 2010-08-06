using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Simple.Data.SqlCe;
using System.IO;

namespace Simple.Data
{
    class ProviderHelper
    {
        public static IConnectionProvider GetProviderByFilename(string filename)
        {
            var extension = (Path.GetExtension(filename) ?? "").ToLower();
            if (extension == ".sdf") return new SqlCeProvider(filename);

            throw new ArgumentException("Unrecognised file.");
        }
    }
}
