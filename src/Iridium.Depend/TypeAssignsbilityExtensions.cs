using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Iridium.Depend
{
    public static class TypeAssignsbilityExtensions
    {
        public static bool IsAssignableTo(this Type type, Type targetType)
        {
            if (targetType.IsGenericTypeDefinition)
                return IsAssignableToGenericType(type, targetType);

            return targetType.GetTypeInfo().IsAssignableFrom(type);
        }

        private static bool IsAssignableToGenericType(Type givenType, Type genericType)
        {
            var interfaceTypes = givenType.GetInterfaces();

            foreach (var it in interfaceTypes)
            {
                if (it.IsGenericType && it.GetGenericTypeDefinition() == genericType)
                    return true;
            }

            if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
                return true;

            Type baseType = givenType.BaseType;

            if (baseType == null)
                return false;

            return IsAssignableToGenericType(baseType, genericType);
        }

        public static bool IsAnonymousType(this object source)
        {
            if (source == null)
                return false;

            var type = source.GetType();

            return type.IsAnonymousType();
            //
            // return type.IsGenericType
            //        && (type.Name.StartsWith("<>", StringComparison.OrdinalIgnoreCase) || type.Name.StartsWith("VB$", StringComparison.OrdinalIgnoreCase))
            //        && (type.Name.Contains("AnonymousType") || type.Name.Contains("AnonType"))
            //        && type.GetCustomAttributes<CompilerGeneratedAttribute>().Any();
        }

        public static bool IsAnonymousType(this Type type)
        {
            if (type == null)
                return false;

            return type.IsGenericType
                   && (type.Name.StartsWith("<>", StringComparison.OrdinalIgnoreCase) || type.Name.StartsWith("VB$", StringComparison.OrdinalIgnoreCase))
                   && (type.Name.Contains("AnonymousType") || type.Name.Contains("AnonType"))
                   && type.GetCustomAttributes<CompilerGeneratedAttribute>().Any();
        }

        public static bool CanBeNull(this Type type)
        {
            return (!type.IsValueType || Nullable.GetUnderlyingType(type) != null);
        }

    }
}