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
using System.Linq.Expressions;
using System.Reflection;

namespace Iridium.Depend
{
    internal class ServiceProvider : IServiceProvider
    {
        private readonly ConcurrentDictionary<Type, ServiceDefinition> _createTypeCache = new ConcurrentDictionary<Type, ServiceDefinition>();
        private readonly ConcurrentDictionary<Type, (Type type, Delegate factory)> _factoryTypeCache = new ConcurrentDictionary<Type, (Type type, Delegate factory)>();

        private readonly ServiceResolver _serviceResolver;
        private readonly ServiceScope _scope;
        private readonly ServiceScope _rootScope;

        public ServiceProvider(ServiceResolver serviceResolver, ServiceScope scope, ServiceScope rootScope = null)
        {
            _serviceResolver = serviceResolver;
            _scope = scope;
            _rootScope = rootScope;
        }

        public T Resolve<T>()
        {
            return (T)_Resolve(typeof(T));
        }

        public T Resolve<T>(params object[] parameters)
        {
            return (T)_Resolve(typeof(T), parameters is { Length: 0 } ? null : ConstructorParameter.GenerateConstructorParameters(parameters).ToArray());
        }

        public object Resolve(Type type, params object[] parameters)
        {
            return _Resolve(type, parameters is { Length: 0 } ? null : ConstructorParameter.GenerateConstructorParameters(parameters).ToArray());
        }

        public T Get<T>() => Resolve<T>();
        public T Get<T>(params object[] parameters) => Resolve<T>(parameters);
        public object Get(Type type, params object[] parameters) => Resolve(type, parameters);

        private object _Resolve(Type type, ConstructorParameter[] parameters = null)
        {
            if (type.IsDeferredType())
            {
                return CreateDeferredValue(type);
            }

            var services = _serviceResolver.Resolve(type);

            if (services == null)
            {
                var obj = _serviceResolver.ResolveDynamic(type, this);

                if (obj == null)
                    return null;
            }

            return _Resolve(services, type, parameters);
        }

        private object _Resolve(List<ServiceDefinition> services, Type type, ConstructorParameter[] parameters)
        {
            object instance;

            foreach (var service in services)
            {
                if (service.RegisteredObject != null)
                {
                    instance = service.RegisteredObject;

                    SetInjectProperties(service.RegisteredObject, service.Type);
                }
                else
                {
                    var scope = service.Lifetime switch
                    {
                        ServiceLifetime.Transient => null,
                        ServiceLifetime.Scoped => _scope ?? throw new Exception("No active scope"),
                        ServiceLifetime.Singleton => (_rootScope ?? _scope) ?? throw new Exception("No active scope"),
                        _ => null
                    };

                    if (scope == null)
                    {
                        instance = _CreateService(type, service, parameters);
                    }
                    else
                    {
                        instance = scope.GetOrStore(service, type, () => _CreateService(type, service, parameters));
                    }

                    if (instance == null)
                        continue;
                }

                foreach (var action in service.AfterResolveActions)
                {
                    action(instance, this);
                }

                return instance;
            }

            throw new Exception($"Could not resolve service {type.Name}");
        }

        private IEnumerable _ResolveEnumerable(Type serviceType)
        {
            var matchingServices = _serviceResolver.ResolveAll(serviceType);

            foreach (var service in matchingServices)
            {
                yield return _Resolve(new List<ServiceDefinition>() {service}, serviceType, null);
            }
        }

        private object _CreateService(Type type, ServiceDefinition serviceDefinition, ConstructorParameter[] parameters = null)
        {
            object obj;

            if (serviceDefinition.Factory != null)
            {
                obj = serviceDefinition.Factory(this, type);
            }
            else
            {
                obj = CallBestConstructor(serviceDefinition, type, parameters);
            }

  //          return null;

            if (obj == null)
                return null;
//                throw new Exception($"Unable to create instance of service {serviceDefinition.Type.Name}");

            SetInjectProperties(obj, serviceDefinition.Type);

            foreach (var action in serviceDefinition.AfterCreateActions)
            {
                action(obj, this);
            }

            return obj;
        }


        private object _Create(Type type, ConstructorParameter[] parameters = null)
        {
            var serviceDefinition = _createTypeCache.GetOrAdd(type, t => new ServiceDefinition(t));

            var obj = CallBestConstructor(serviceDefinition, type, parameters);

            if (obj == null)
                throw new Exception($"No constructors found for type {type.Name}");

            SetInjectProperties(obj, type);

            return obj;
        }

        public object Create(Type type)
        {
            return _Create(type);
        }

        public object Create(Type type, params object[] parameters)
        {
            return _Create(type, parameters is { Length: 0 } ? null : ConstructorParameter.GenerateConstructorParameters(parameters).ToArray());
        }

        public T Create<T>() where T : class
        {
            return (T)_Create(typeof(T));
        }

        public T Create<T>(params object[] parameters) where T : class
        {
            return (T)Create(typeof(T), parameters);
        }

        public void UpdateDependencies(object o)
        {
            SetInjectProperties(o);
        }

        private object CallBestConstructor(ServiceDefinition serviceDefinition, Type type, ConstructorParameter[] parameters)
        {
            ConstructorCandidate bestConstructorCandidate;

            if (parameters == null && !serviceDefinition.IsOpenGenericType && serviceDefinition.BestConstructorCandidate != null)
            {
                bestConstructorCandidate = serviceDefinition.BestConstructorCandidate;
            }
            else
            {
                bestConstructorCandidate = serviceDefinition.ServiceConstructors
                    .Select(sc => new ConstructorCandidate(_serviceResolver, sc.Constructor(type), parameters))
                    .Where(candidate => candidate.MatchScore >= 0)
                    .OrderByDescending(c => c.MatchScore)
                    .FirstOrDefault();
            }

            return bestConstructorCandidate?.Invoke(this);
        }

        private void SetInjectProperties(object o, Type type = null)
        {
            if (o == null)
                return;

            type ??= o.GetType();

            var injectProperties = type.GetInjectProperties();

            if (injectProperties == null)
                return;

            foreach (var property in injectProperties)
            {
                if (_serviceResolver.CanResolve(property.PropertyType))
                {
                    property.SetValue(o, Get(property.PropertyType));
                }
                else if (property.PropertyType.IsDeferredType())
                {
                    property.SetValue(o, CreateDeferredValue(property.PropertyType));
                }
            }
        }

        private static readonly MethodInfo _getMethod = typeof(ServiceProvider).GetMethod("Get", Type.EmptyTypes);

        private object CreateDeferredValue(Type type)
        {
            if (type.IsDeferredListType(out var itemType))
            {
                var objects = _ResolveEnumerable(itemType);

                return type.CreateDeferredList(objects);
            }

            var deferredType = type.DeferredType();

            var (lazyType, factory) = _factoryTypeCache.GetOrAdd(type, t =>
            {
                //var getMethod = typeof(ServiceProvider).GetMethod(nameof(ServiceProvider.Get), new[] { typeof(Type), typeof(object[]) });

                var getMethod = _getMethod.MakeGenericMethod(deferredType);
                var methodType = typeof(Func<>).MakeGenericType(deferredType);

                var factory = Delegate.CreateDelegate(methodType, this, getMethod);
                
                // var lambda = Expression.Lambda(
                //     Expression.TypeAs(
                //         Expression.Call(
                //             Expression.Constant(this),
                //             getMethod,
                //             Expression.Constant(deferredType),
                //             Expression.Constant(Array.Empty<object>())
                //         ),
                //         deferredType
                //     )
                // ).Compile();

                return (type.GetGenericTypeDefinition().MakeGenericType(deferredType), f: factory);
            });

            return Activator.CreateInstance(lazyType, factory);
        }


        public bool CanResolve(Type type)
        {
            return _serviceResolver.CanResolve(type);
        }

        public IServiceProvider CreateScope()
        {
            return new ServiceProvider(_serviceResolver, new ServiceScope(), _rootScope);
        }

        public void Dispose()
        {
            _scope.Dispose();
        }
    }
}