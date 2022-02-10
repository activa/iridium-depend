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
using System.Linq;
using System.Reflection;

namespace Iridium.Depend
{
    internal class ServiceConstructor
    {
        private readonly ConcurrentDictionary<Type, ConstructorInfo> _genericConstructorsCache = new ConcurrentDictionary<Type, ConstructorInfo>();

        private readonly ServiceDefinition _serviceDefinition;
        private readonly ConstructorInfo _constructorInfo;
        private ConstructorCandidate _constructorCandidate;

        public ServiceConstructor(ServiceDefinition serviceDefinition, ConstructorInfo constructorInfo)
        {
            _serviceDefinition = serviceDefinition;
            _constructorInfo = constructorInfo;
        }

        public ConstructorInfo Constructor(Type type)
        {
            if (!_serviceDefinition.IsOpenGenericType)
                return _constructorInfo;

            if (!type.IsConstructedGenericType)
                throw new Exception("Can't create object from open generic type");

            return _genericConstructorsCache.GetOrAdd(
                type,
                t => _serviceDefinition.Type
                    .MakeGenericType(t.GenericTypeArguments)
                    .GetConstructors()
                    .FirstOrDefault(c => c.MetadataToken == _constructorInfo.MetadataToken)
                );
        }

        public ConstructorCandidate ConstructorCandidate => _constructorCandidate;

        public void PreResolve(ServiceResolver serviceResolver)
        {
            _constructorCandidate = new ConstructorCandidate(serviceResolver, _constructorInfo, null);
        }
    }
}