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
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Iridium.Depend
{
    public static class DeferredTypeExtensions
    {
        private class ListTypeInfo
        {
            public ListTypeInfo(Type listType, Type elementType)
            {
                ElementType = elementType;
                ListType = listType;
            }

            public readonly Type ElementType;
            public readonly Type ListType;
        }

        private static readonly ConcurrentDictionary<Type, ListTypeInfo> _listTypes = new();

        public static bool IsLazyType(this Type type)
        {
            if (type.IsDeferredListType())
                return false;

            if (!type.IsGenericType || type.GenericTypeArguments.Length != 1)
                return false;

            var genericTypeDefinition = type.GetGenericTypeDefinition();

            if (genericTypeDefinition == typeof(Lazy<>) || genericTypeDefinition == typeof(Live<>))
                return true;

            return false;
        }

        public static bool IsDeferredListType(this Type type)
        {
            return type.IsDeferredListType(out _);
        }

        public static bool IsDeferredType(this Type type)
        {
            return type.IsDeferredListType() || type.IsLazyType();
        }

        private static bool IsValidElementType(Type elementType)
        {
            return elementType != typeof(object) && !elementType.IsValueType && elementType != typeof(string);
        }

        private static ListTypeInfo _DeferredListType(Type type)
        {
            if (type.IsArray)
            {
                var arrayElementType = type.GetElementType()!;

                if (IsValidElementType(arrayElementType))
                    return new ListTypeInfo(typeof(Array), arrayElementType);

                return null;
            }

            Type enumerableInterface;

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                enumerableInterface = type;
            else
                enumerableInterface = type.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));

            if (enumerableInterface == null)
                return null;

            var elementType = enumerableInterface.GenericTypeArguments[0];
            
            if (!IsValidElementType(elementType))
                return null;

            if (type.IsGenericType && type.IsInterface)
            {
                var gen = type.GetGenericTypeDefinition();

                if (gen == typeof(IEnumerable<>) || gen == typeof(IList<>) || gen == typeof(ICollection<>))
                    return new ListTypeInfo(typeof(List<>).MakeGenericType(elementType), elementType);
            }

            var constructor = type.GetConstructors().FirstOrDefault(c =>
            {
                var parameters = c.GetParameters();
                
                return parameters.Length == 1 && parameters[0].ParameterType.IsGenericType && parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(IEnumerable<>);
            });

            if (constructor != null)
                return new ListTypeInfo(type, elementType);

            return null;
        }

        private static ListTypeInfo GetListTypeInfo(Type type)
        {
            return _listTypes.GetOrAdd(type, _DeferredListType);
        }

        public static bool IsDeferredListType(this Type type, out Type elementType)
        {
            ListTypeInfo listTypeInfo = GetListTypeInfo(type);

            if (listTypeInfo == null)
            {
                elementType = null;
                return false;
            }

            elementType = listTypeInfo.ElementType;

            return true;
        }

        private static (Type listType, Type itemType) DeferredListType(this Type type)
        {
            var listTypeInfo = GetListTypeInfo(type);

            return (listTypeInfo?.ListType, listTypeInfo?.ElementType);
        }

        public static Type DeferredType(this Type type)
        {
            if (type.IsArray)
                return type.GetElementType();

            return type.GenericTypeArguments[0];
        }

        private static object ToTypedEnumerable(IEnumerable enumerable, Type toType)
        {
            var castMethod = typeof(Enumerable).GetMethod(nameof(Enumerable.Cast))!.MakeGenericMethod(toType);

            return castMethod.Invoke(null, new[] { enumerable });
        }

        public static object CreateDeferredList(this Type type, IEnumerable list)
        {
            var (listType, itemType) = type.DeferredListType();

            if (listType == typeof(Array))
            {
                object[] objects = list.Cast<object>().ToArray();
                
                var array = Array.CreateInstance(itemType, objects.Length);
                
                Array.Copy(objects, array, objects.Length);

                return array;
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                return ToTypedEnumerable(list, itemType);
            }
            else
            {
                return Activator.CreateInstance(listType, ToTypedEnumerable(list, itemType));
            }
        }
    }
}