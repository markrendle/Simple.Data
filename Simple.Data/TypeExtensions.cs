namespace Simple.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public static class TypeExtensions
    {
        public static bool IsGenericCollection(this Type type)
        {
            return type.IsGenericType &&
                (type.GetGenericTypeDefinition() == typeof(ICollection<>) ||
                   type.GetGenericTypeDefinition().GetInterfaces()
                       .Where(i => i.IsGenericType)
                       .Select(i => i.GetGenericTypeDefinition())
                       .Contains(typeof(ICollection<>)));
        }

        public static MethodInfo GetInterfaceMethod(this Type type, string name)
        {
            return type.GetMethod(name)
                   ??
                   type.GetInterfaces()
                       .Select(t => t.GetInterfaceMethod(name))
                       .FirstOrDefault(m => m != null);
        }
    }
}