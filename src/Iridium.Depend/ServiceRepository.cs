#region License
//=============================================================================
// Iridium-Depend - Portable .NET Productivity Library 
//
// Copyright (c) 2008-2021 Philippe Leybaert
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
    public class ServiceRepository : IServiceRepository
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
            var serviceToRemove = ServiceDefinitions.FirstOrDefault(svc => svc.Type == type);

            if (serviceToRemove != null)
                RemoveService(serviceToRemove);
        }

        public IServiceRegistrationResult<T,T> Register<T>()
        {
            var serviceDefinition = AddService(new ServiceDefinition(typeof(T)));

            return new ServiceRegistrationResult<T,T>(this, serviceDefinition);
        }

        public IServiceRegistrationResult Register(Type type)
        {
            var serviceDefinition = new ServiceDefinition(type);

            AddService(serviceDefinition);

            return new ServiceRegistrationResult(this, serviceDefinition);
        }

        public IServiceRegistrationResult Register(Type type, object service)
        {
            var serviceDefinition = new ServiceDefinition(type, service);

            AddService(serviceDefinition);

            return new ServiceRegistrationResult(this, serviceDefinition);
        }

        public IServiceRegistrationResult<T,T> Register<T>(T service)
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));

            var serviceDefinition = new ServiceDefinition(typeof(T), service);

            AddService(serviceDefinition);

            return new ServiceRegistrationResult<T,T>(this, serviceDefinition);
        }

        public IServiceRegistrationResult Register(Type registrationType, Func<object> factoryMethod)
        {
            if (factoryMethod == null)
                throw new ArgumentNullException(nameof(factoryMethod));

            var serviceDefinition = new ServiceDefinition(registrationType, factoryMethod: (repo, type) => factoryMethod());

            AddService(serviceDefinition);

            return new ServiceRegistrationResult(this, serviceDefinition);
        }

        public IServiceRegistrationResult Register(Type registrationType, Func<Type, object> factoryMethod)
        {
            if (factoryMethod == null)
                throw new ArgumentNullException(nameof(factoryMethod));

            var serviceDefinition = new ServiceDefinition(registrationType, factoryMethod: (repo,type) => factoryMethod(type));

            AddService(serviceDefinition);

            return new ServiceRegistrationResult(this, serviceDefinition);
        }

        public IServiceRegistrationResult Register(Type type, Func<IServiceProvider, Type, object> factoryMethod)
        {
            if (factoryMethod == null)
                throw new ArgumentNullException(nameof(factoryMethod));

            var serviceDefinition = new ServiceDefinition(type, factoryMethod: factoryMethod);

            AddService(serviceDefinition);

            return new ServiceRegistrationResult(this, serviceDefinition);
        }

        public IServiceRegistrationResult<T,T> Register<T>(Func<T> factoryMethod)
        {
            if (factoryMethod == null)
                throw new ArgumentNullException(nameof(factoryMethod));

            var serviceDefinition = new ServiceDefinition(typeof(T), factoryMethod:(repo,type) => factoryMethod());

            AddService(serviceDefinition);

            return new ServiceRegistrationResult<T,T>(this, serviceDefinition);
        }

        public IServiceRegistrationResult<T,T> Register<T>(Func<IServiceProvider, T> factoryMethod)
        {
            if (factoryMethod == null)
                throw new ArgumentNullException(nameof(factoryMethod));

            var serviceDefinition = new ServiceDefinition(typeof(T), factoryMethod: (repo,type) => factoryMethod(repo));

            AddService(serviceDefinition);

            return new ServiceRegistrationResult<T,T>(this, serviceDefinition);
        }

        public Type[] ResolvableTypes()
        {
            lock (_services)
                return _services.SelectMany(svc => svc.RegistrationTypes).ToArray();
        }

        public IServiceProvider CreateServiceProvider(bool allowMultipleSingletonCreations = true)
        {
            return new ServiceProvider(
                new ServiceResolver(this), 
                new ServiceScope()
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
    }
}