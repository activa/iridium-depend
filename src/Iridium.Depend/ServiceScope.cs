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

namespace Iridium.Depend
{
    internal class ServiceScope : IDisposable
    {
        private bool _disposed;

        private readonly ConcurrentDictionary<ServiceDefinition, object> _serviceInstances = new();
        private readonly ConcurrentDictionary<(ServiceDefinition serviceDefinition, TypeCollection typeArgs), object> _genericServiceInstances = new();

        public object GetOrStore(ServiceDefinition service, Type type, Func<object> factory)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(ServiceScope));

            object instance;

            if (service.IsOpenGenericType)
            {
                var typeArgs = new TypeCollection(type.GetGenericArguments());

                if (!_genericServiceInstances.TryGetValue((service, typeArgs), out instance))
                {
                    lock (_genericServiceInstances)
                    {
                        if (!_genericServiceInstances.TryGetValue((service, typeArgs), out instance))
                        {
                            instance = factory();

                            if (instance != null)
                                _genericServiceInstances.TryAdd((service, typeArgs), instance);
                        }
                    }
                }
            }
            else
            {
                if (!_serviceInstances.TryGetValue(service, out instance))
                {
                    lock (_serviceInstances)
                    {
                        if (!_serviceInstances.TryGetValue(service, out instance))
                        {
                            instance = factory();

                            if (instance != null)
                                _serviceInstances.TryAdd(service, instance);
                        }
                    }
                }
            }

            return instance;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            foreach (var disposable in _serviceInstances.Where(svc => !svc.Key.SkipDispose).Select(svc => svc.Value).OfType<IDisposable>())
            {
                disposable.Dispose();
            }

            foreach (var disposable in _genericServiceInstances.Where(svc => !svc.Key.serviceDefinition.SkipDispose).Select(svc => svc.Value).OfType<IDisposable>())
            {
                disposable.Dispose();
            }

            _serviceInstances.Clear();
            _genericServiceInstances.Clear();

            _disposed = true;
        }
    }
}