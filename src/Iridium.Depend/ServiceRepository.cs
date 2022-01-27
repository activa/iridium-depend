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
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Iridium.Depend
{
    public class ServiceRepository : IServiceRepository
    {
        public static ServiceRepository Default = new ServiceRepository();

        private readonly List<ServiceDefinition> _services = new List<ServiceDefinition>();
        private readonly ConcurrentDictionary<Type, PropertyInfo[]> _injectProperties = new ConcurrentDictionary<Type, PropertyInfo[]>();
        private readonly ConcurrentDictionary<Type, List<ServiceDefinition>> _cache = new ConcurrentDictionary<Type, List<ServiceDefinition>>();
        private readonly ConcurrentDictionary<Type, (Type type, Delegate factory)> _factoryTypeCache = new ConcurrentDictionary<Type, (Type type, Delegate factory)>();
        private readonly ServiceResolveStrategy _resolveStrategy = ServiceResolveStrategy.First;

        public ServiceRepository()
        {
        }

        public ServiceRepository(ServiceResolveStrategy resolveStrategy)
        {
            _resolveStrategy = resolveStrategy;
        }

        public ServiceResolveStrategy ResolveStrategy => _resolveStrategy;

        public T Get<T>()
        {
            return (T) _Get(typeof(T));
        }

        public T Get<T>(params object[] parameters)
        {
            return (T) _Get(typeof(T), ConstructorParameter.GenerateConstructorParameters(parameters).ToArray());
        }

        public T Get<T, TParam1>(TParam1 param)
        {
            return (T) _Get(typeof(T), new ConstructorParameter[] {new ConstructorParameter<TParam1>(param)});
        }

        public T Get<T, TParam1, TParam2>(TParam1 param1, TParam2 param2)
        {
            return (T)_Get(typeof(T), new ConstructorParameter[] { new ConstructorParameter<TParam1>(param1), new ConstructorParameter<TParam2>(param2) });
        }

        public IEnumerable<T> GetAll<T>()
        {
            return _GetAll(typeof(T)).Cast<T>();
        }

        internal bool CanResolve(Type type) => GetMatchingServices(type).Count > 0;

        private object CallBestConstructor(IEnumerable<ConstructorInfo> constructors, ConstructorParameter[] parameters)
        {
            parameters ??= new ConstructorParameter[0];

            var bestCandidate = constructors
                .Select(constructor => new ConstructorCandidate(this, constructor, parameters))
                .Where(candidate => candidate.MatchScore >= 0)
                .OrderByDescending(c => c.MatchScore)
                .FirstOrDefault();

            return bestCandidate?.Invoke();
        }

        private List<ServiceDefinition> GetMatchingServices(Type type)
        {
            return _cache.GetOrAdd(type, t =>
            {
                lock (_services)
                    return _services.Where(svc => svc.IsMatch(t)).ToList();
            });
        }

        private object _Get(Type type, ConstructorParameter[] parameters = null)
        {
            if (type.IsFactoryValue())
            {
                return CreateFactoryValue(type);
            }
            
            foreach (var serviceDefinition in GetMatchingServices(type))
            {
                var obj = _Get(type, serviceDefinition, parameters);

                if (obj != null)
                    return obj;
            }

            return null;
        }

        private IEnumerable<object> _GetAll(Type type)
        {
            if (type.IsFactoryValue())
            {
                return new[] { CreateFactoryValue(type) };
            }

            return GetMatchingServices(type).Select(svc => _Get(type, svc)).Where(o => o != null);
        }

        private object _Get(Type type, ServiceDefinition serviceDefinition, ConstructorParameter[] parameters = null)
        {
            var obj = serviceDefinition.GetSingletonObject(type);

            if (obj != null)
            {
                SetInjectProperties(obj);
                return obj;
            }

            if (serviceDefinition.Factory != null)
                obj = serviceDefinition.Factory(this, type);
            else
                obj = CallBestConstructor(serviceDefinition.MatchingConstructors(type), parameters);

            if (obj != null)
            {
                SetInjectProperties(obj);

                if (serviceDefinition.Singleton)
                {
                    serviceDefinition.SetSingletonObject(type, obj);
                }

                return obj;
            }

            return null;
        }

        public object Get(Type type, params object[] parameters)
        {
            return _Get(type, parameters.Select(p => new ConstructorParameter(p)).ToArray());
        }

        private object _Create(Type type, ConstructorParameter[] parameters = null)
        {
            var obj = CallBestConstructor(new ServiceDefinition(type).Constructors, parameters);

            if (obj == null)
                return null;

            SetInjectProperties(obj);

            return obj;
        }

        public object Create(Type type)
        {
            return _Create(type);
        }

        public object Create(Type type, params object[] parameters)
        {
            return _Create(type, ConstructorParameter.GenerateConstructorParameters(parameters).ToArray());
        }

        public T Create<T>() where T : class
        {
            return (T)_Create(typeof(T));
        }

        public T Create<T>(params object[] parameters) where T : class
        {
            return (T) Create(typeof(T), parameters);
        }

        public T Create<T,TParam1>(TParam1 param) where T : class
        {
            return (T)_Create(typeof(T), new ConstructorParameter[] {new ConstructorParameter<TParam1>(param)});
        }

        public T Create<T, TParam1, TParam2>(TParam1 param1, TParam2 param2) where T : class
        {
            return (T)_Create(typeof(T), new ConstructorParameter[] { new ConstructorParameter<TParam1>(param1), new ConstructorParameter<TParam2>(param2) });
        }

        public void UpdateDependencies(object o)
        {
            SetInjectProperties(o);
        }

        private void SetInjectProperties(object o)
        {
            if (o == null)
                return;

            var type = o.GetType();

            var injectProperties = _injectProperties.GetOrAdd(type, t => type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanWrite && p.GetCustomAttribute<InjectAttribute>() != null).ToArray());

            foreach (var property in injectProperties)
            {
                if (CanResolve(property.PropertyType))
                {
                    property.SetValue(o, Get(property.PropertyType));
                }
                else if (property.PropertyType.IsFactoryValue())
                {
                    property.SetValue(o, CreateFactoryValue(property.PropertyType));
                }
            }
        }

        private ServiceDefinition AddService(ServiceDefinition service)
        {
            lock (_services)
            {
                if (_resolveStrategy == ServiceResolveStrategy.First)
                    _services.Add(service);
                else
                    _services.Insert(0,service);
            }

            _cache.Clear();

            return service;
        }

        public void UnRegister<T>()
        {
            UnRegister(typeof(T));
        }

        public void UnRegister(Type type)
        {
            lock (_services)
                _services.RemoveAll(svc => svc.IsMatch(type));

            _cache.Clear();
        }

        public void Refresh(bool disposePrevious = false)
        {
            foreach (var service in _services)
            {
                service.ClearStoredSingleton(disposePrevious);
            }
        }

        public void Refresh<T>(bool disposePrevious = false)
        {
            Refresh(typeof(T), disposePrevious);
        }

        public void Refresh(Type type, bool disposePrevious = false)
        {
            foreach (var serviceDefinition in GetMatchingServices(type))
            {
                serviceDefinition.ClearStoredSingleton(disposePrevious);
            }
        }

        public IServiceRegistrationResult Register<T>()
        {
            var serviceDefinition = AddService(new ServiceDefinition(typeof(T)));

            return new ServiceRegistrationResult(this, serviceDefinition);
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

        public IServiceRegistrationResult Register<T>(T service)
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));

            var serviceDefinition = new ServiceDefinition(typeof(T), service);

            AddService(serviceDefinition);

            return new ServiceRegistrationResult(this, serviceDefinition);
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

        public IServiceRegistrationResult Register(Type type, Func<IServiceRepository, Type, object> factoryMethod)
        {
            if (factoryMethod == null)
                throw new ArgumentNullException(nameof(factoryMethod));

            var serviceDefinition = new ServiceDefinition(type, factoryMethod: factoryMethod);

            AddService(serviceDefinition);

            return new ServiceRegistrationResult(this, serviceDefinition);
        }

        public IServiceRegistrationResult Register<T>(Func<T> factoryMethod)
        {
            if (factoryMethod == null)
                throw new ArgumentNullException(nameof(factoryMethod));

            var serviceDefinition = new ServiceDefinition(typeof(T), factoryMethod:(repo,type) => factoryMethod());

            AddService(serviceDefinition);

            return new ServiceRegistrationResult(this, serviceDefinition);
        }

        public IServiceRegistrationResult Register<T>(Func<IServiceRepository, T> factoryMethod)
        {
            if (factoryMethod == null)
                throw new ArgumentNullException(nameof(factoryMethod));

            var serviceDefinition = new ServiceDefinition(typeof(T), factoryMethod: (repo,type) => factoryMethod(repo));

            AddService(serviceDefinition);

            return new ServiceRegistrationResult(this, serviceDefinition);
        }

        public Type[] RegisteredTypes()
        {
            lock (_services)
                return _services.Select(svc => svc.RegistrationType).ToArray();
        }

        internal void RemoveConflicting(Type registrationType, ServiceDefinition svcDef)
        {
            lock (_services)
                _services.RemoveAll(svc => !ReferenceEquals(svcDef, svc) && svc.IsMatch(registrationType));
        }

        private object CreateFactoryValue(Type type)
        {
            var (lazyType, factory) = _factoryTypeCache.GetOrAdd(type, t =>
            {
                var targetType = t.GetGenericArguments()[0];

                var getMethod = typeof(ServiceRepository).GetMethod("Get", new[] { typeof(Type), typeof(object[]) });

                var methodCall = Expression.Call(Expression.Constant(this), getMethod, Expression.Constant(targetType), Expression.Constant(new object[0]));

                var lambda = Expression.Lambda(Expression.TypeAs(methodCall, targetType)).Compile();

                return (type.GetGenericTypeDefinition().MakeGenericType(targetType), lambda);
            });

            return Activator.CreateInstance(lazyType, factory);
        }

    }
}