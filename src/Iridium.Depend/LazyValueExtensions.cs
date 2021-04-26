using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Iridium.Depend
{
    public static class LazyValueExtensions
    {
        public static object CreateFactoryValue(this Type type, ServiceRepository repo)
        {
            var targetType = type.GetGenericArguments()[0];

            var method = typeof(ServiceRepository).GetMethod("Get", new[] { typeof(Type), typeof(object[]) });

            var methodCall = Expression.Call(Expression.Constant(repo), method, Expression.Constant(targetType), Expression.Constant(new object[0]));

            var lambda = Expression.Lambda(Expression.TypeAs(methodCall, targetType)).Compile();

            var lazyType = type.GetGenericTypeDefinition().MakeGenericType(targetType);

            return Activator.CreateInstance(lazyType, lambda);
        }

        public static bool IsFactoryValue(this Type type)
        {
            if (!type.IsGenericType)
                return false;

            var genericTypeDefinition = type.GetGenericTypeDefinition();

            return (genericTypeDefinition == typeof(Lazy<>) || genericTypeDefinition == typeof(Live<>));
        }
    }
}