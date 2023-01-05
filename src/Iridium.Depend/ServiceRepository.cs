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
using System.Collections.Generic;
using System.Linq;

namespace Iridium.Depend
{
    public partial class ServiceRepository : IServiceRepository, IDisposable
    {
        public static ServiceRepository _default;

        public static ServiceRepository Default => _default ??= new ServiceRepository();

        private readonly List<ServiceDefinition> _services = new List<ServiceDefinition>();

        public ServiceRepository()
        {
        }

        private ServiceDefinition AddService(ServiceDefinition service)
        {
            lock (_services)
            {
                _services.Add(service);
            }

            TriggerChangeEvent();

            return service;
        }

        internal void RemoveService(ServiceDefinition service)
        {
            lock (_services)
                _services.Remove(service);

            TriggerChangeEvent();
        }

        public void UnRegister<T>()
        {
            UnRegister(typeof(T));
        }

        public void UnRegister(Type type)
        {
            lock (_services)
            {
                var serviceToRemove = _services.FirstOrDefault(svc => svc.Type == type);

                if (serviceToRemove != null)
                    RemoveService(serviceToRemove);
            }
        }

        public void UnRegister(object instance)
        {
            lock (_services)
            {
                var serviceToRemove = _services.FirstOrDefault(svc => object.Equals(svc.RegisteredObject, instance));

                if (serviceToRemove != null)
                    RemoveService(serviceToRemove);
            }
        }

        public IServiceRegistrationResult<T,T> Register<T>()
        {
            return new ServiceRegistrationResult<T,T>(this, AddService(new ServiceDefinition(typeof(T))));
        }

        public IServiceRegistrationResult Register(Type type)
        {
            return new ServiceRegistrationResult(this, AddService(new ServiceDefinition(type)));
        }

        public IServiceRegistrationResult Register(Type type, object service)
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));

            return new ServiceRegistrationResult(this, AddService(new ServiceDefinition(type, service)));
        }

        public IServiceRegistrationResult<T,T> Register<T>(T service)
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));

            return new ServiceRegistrationResult<T,T>(this, AddService(new ServiceDefinition(typeof(T), service)));
        }

        public IServiceRegistrationResult Register(Type registrationType, Func<object> factoryMethod)
        {
            if (factoryMethod == null)
                throw new ArgumentNullException(nameof(factoryMethod));

            return new ServiceRegistrationResult(this, AddService(new ServiceDefinition(registrationType, factoryMethod: (repo, type) => factoryMethod())));
        }

        public IServiceRegistrationResult Register(Type registrationType, Func<Type, object> factoryMethod)
        {
            if (factoryMethod == null)
                throw new ArgumentNullException(nameof(factoryMethod));

            return new ServiceRegistrationResult(this, AddService(new ServiceDefinition(registrationType, factoryMethod: (repo,type) => factoryMethod(type))));
        }

        public IServiceRegistrationResult Register(Type type, Func<IServiceProvider, Type, object> factoryMethod)
        {
            if (factoryMethod == null)
                throw new ArgumentNullException(nameof(factoryMethod));

            return new ServiceRegistrationResult(this, AddService(new ServiceDefinition(type, factoryMethod: factoryMethod)));
        }

        public IServiceRegistrationResult<T,T> Register<T>(Func<T> factoryMethod)
        {
            if (factoryMethod == null)
                throw new ArgumentNullException(nameof(factoryMethod));

            return new ServiceRegistrationResult<T,T>(this, AddService(new ServiceDefinition(typeof(T), factoryMethod:(repo,type) => factoryMethod())));
        }

        public IServiceRegistrationResult<T,T> Register<T>(Func<IServiceProvider, T> factoryMethod)
        {
            if (factoryMethod == null)
                throw new ArgumentNullException(nameof(factoryMethod));

            return new ServiceRegistrationResult<T,T>(this, AddService(new ServiceDefinition(typeof(T), factoryMethod: (repo,type) => factoryMethod(repo))));
        }

        public Type[] ResolvableTypes()
        {
            lock (_services)
                return _services.SelectMany(svc => svc.RegistrationTypes).ToArray();
        }

        private readonly ServiceScope _singletonScope = new ServiceScope();

        public IServiceProvider CreateServiceProvider()
        {
            return new ServiceProvider(
                new ServiceResolver(this),
                null,
                _singletonScope
                );
        }

        internal List<ServiceDefinition> ServiceDefinitions
        {
            get
            {
                lock (_services)
                {
                    return new List<ServiceDefinition>(_services);
                }
            }
        }

        internal void TriggerChangeEvent() => Changed?.Invoke(this, EventArgs.Empty);

        internal event EventHandler Changed;

        public void Dispose()
        {
            _singletonScope.Dispose();
        }
    }

}