namespace Simple.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

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
    }
}