using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Iridium.Depend
{
    public static class LazyValueExtensions
    {
        public static bool IsFactoryValue(this Type type)
        {
            if (!type.IsGenericType)
                return false;

            var genericTypeDefinition = type.GetGenericTypeDefinition();

            return (genericTypeDefinition == typeof(Lazy<>) || genericTypeDefinition == typeof(Live<>));
        }
    }
}