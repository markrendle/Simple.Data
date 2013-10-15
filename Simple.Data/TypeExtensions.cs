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

        public static MethodInfo GetOperatorConversionMethod(this Type conversionOwnerType, Type convertFromType)
        {
            var converter = conversionOwnerType.GetMethod("op_Implicit", new[] { convertFromType });
            if (converter != null)
                return converter;

            converter = conversionOwnerType.GetMethod("op_Explicit", new[] { convertFromType });
            if (converter != null)
                return converter;

            return null;
        }
    }
}