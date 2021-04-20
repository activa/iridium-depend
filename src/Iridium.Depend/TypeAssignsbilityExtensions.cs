using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
    }
}