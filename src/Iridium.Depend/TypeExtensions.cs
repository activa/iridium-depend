#region License
//=============================================================================
// Iridium-Depend - Portable .NET Productivity Library 
//
// Copyright (c) 2008-2022 Philippe Leybaert
//
// Permission is hereby granted, free of charge, to any person obtaining a copy 
// of this software and associated documentation files (the "Software"), to deal 
// in the Software without restriction, including without limitation the rights 
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
// copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in 
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// IN THE SOFTWARE.
//=============================================================================
#endregion

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Iridium.Depend
{
    public static class TypeExtensions
    {
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _injectProperties = new ConcurrentDictionary<Type, PropertyInfo[]>();

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

            var baseType = givenType.BaseType;

            if (baseType == null)
                return false;

            return IsAssignableToGenericType(baseType, genericType);
        }

        public static bool IsAnonymousType(this object source)
        {
            if (source == null)
                return false;

            return source.GetType().IsAnonymousType();
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

        public static PropertyInfo[] GetInjectProperties(this Type type)
        {
            return _injectProperties.GetOrAdd(
                type, 
                t =>
                {
                    var injectProperties = 
                        type
                        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .Where(p => p.CanWrite && p.GetCustomAttribute<InjectAttribute>() != null)
                        .ToArray();

                    return injectProperties.Length == 0 ? null : injectProperties;
                });
        }

        public static (ParameterInfo parameter, bool required)[] GetInjectParameters(ConstructorInfo constructor)
        {
            ParameterInfo[] parameters = constructor.GetParameters();

            return parameters.Select(p => (p, p.GetCustomAttribute<InjectAttribute>() is { Required: true })).ToArray();
        }
    }
}