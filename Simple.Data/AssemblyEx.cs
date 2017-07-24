using System;
using System.Reflection;

namespace Shitty.Data
{
    public static class AssemblyEx
    {
        public static string GetFullName(this Assembly assembly)
        {
            return assembly.FullName.Substring(0, assembly.FullName.IndexOf(",", StringComparison.OrdinalIgnoreCase));
        }
    }
}