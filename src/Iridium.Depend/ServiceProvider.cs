using System;
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

        public ServiceResolver ServiceResolver => _serviceResolver;

        public T Resolve<T>()
        {
            return (T)_Resolve(typeof(T));
        }

        public T Resolve<T>(params object[] parameters)
        {
            return (T)_Resolve(typeof(T), ConstructorParameter.GenerateConstructorParameters(parameters).ToArray());
        }

        public object Resolve(Type type, params object[] parameters)
        {
            return _Resolve(type, parameters.Select(p => new ConstructorParameter(p)).ToArray());
        }

        public T Get<T>() => Resolve<T>();
        public T Get<T>(params object[] parameters) => Resolve<T>(parameters);
        public object Get(Type type, params object[] parameters) => Resolve(type, parameters);

        private object _Resolve(Type type, ConstructorParameter[] parameters = null)
        {
            if (parameters is { Length: 0 })
                parameters = null;

            if (type.IsDeferredType())
            {
                return CreateFactoryValue(type);
            }

            var service = _serviceResolver.Resolve(type);

            if (service == null)
                return default;

            object instance;

            if (service.RegisteredObject != null)
            {
                instance = service.RegisteredObject;

                SetInjectProperties(service.RegisteredObject, service.Type);
            }
            else
            {
                ServiceScope scope = service.Lifetime switch
                {
                    ServiceLifetime.Transient => null,
                    ServiceLifetime.Scoped => _scope,
                    ServiceLifetime.Singleton => (_rootScope ?? _scope),
                    _ => null
                };

                if (scope == null)
                {
                    instance = _CreateService(type, service, parameters);
                }
                else
                {
                    instance = scope.GetOrStore(service, type, _CreateService, parameters);
                }
            }

            foreach (var action in service.AfterResolveActions)
            {
                action(instance, this);
            }

            return instance;
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

            if (obj == null)
                throw new Exception($"Unable to create instance of service {serviceDefinition.Type.Name}");

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
            return _Create(type, ConstructorParameter.GenerateConstructorParameters(parameters).ToArray());
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
                else if (property.PropertyType.IsDeferredType() && _serviceResolver.CanResolve(property.PropertyType.DeferredType()))
                {
                    property.SetValue(o, CreateFactoryValue(property.PropertyType));
                }
            }
        }

        private object CreateFactoryValue(Type type)
        {
            var (lazyType, factory) = _factoryTypeCache.GetOrAdd(type, t =>
            {
                var targetType = t.GetGenericArguments()[0];

                var getMethod = typeof(ServiceProvider).GetMethod(nameof(ServiceProvider.Get), new[] { typeof(Type), typeof(object[]) });

                var lambda = Expression.Lambda(
                    Expression.TypeAs(
                        Expression.Call(
                            Expression.Constant(this),
                            getMethod,
                            Expression.Constant(targetType),
                            Expression.Constant(Array.Empty<object>())
                        ),
                        targetType
                    )
                ).Compile();

                return (type.GetGenericTypeDefinition().MakeGenericType(targetType), lambda);
            });

            return Activator.CreateInstance(lazyType, factory);
        }

        public bool CanResolve(Type type)
        {
            return _serviceResolver.CanResolve(type);
        }

        public IServiceProvider CreateScope()
        {
            return new ServiceProvider(_serviceResolver, new ServiceScope(), _rootScope ?? _scope);
        }

        public void Dispose()
        {
            _scope.Dispose();
        }
    }
}