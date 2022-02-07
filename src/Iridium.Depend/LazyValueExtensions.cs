using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;

namespace Iridium.Depend
{
    public static class LazyValueExtensions
    {
        public static bool IsDeferredType(this Type type)
        {
            if (!type.IsGenericType) 
                return false;

            var genericTypeDefinition = type.GetGenericTypeDefinition();

            return genericTypeDefinition == typeof(Lazy<>) || genericTypeDefinition == typeof(Live<>) || genericTypeDefinition == typeof(IEnumerable<>);
        }

        public static Type DeferredType(this Type type)
        {
            return type.GenericTypeArguments[0];
        }
    }
}