using System;
using System.Reflection;

namespace Simple.Data
{
    public static class AssemblyEx
    {
        public static string GetFullName(this Assembly assembly)
        {
            return assembly.FullName.Substring(0, assembly.FullName.IndexOf(",", StringComparison.OrdinalIgnoreCase));
        }
    }
}