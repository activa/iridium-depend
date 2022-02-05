using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Iridium.Depend
{
    internal class ServiceScope : IServiceScope
    {
        public ServiceResolver ServiceResolver { get; }
        public ServiceScope RootScope { get; }

        // stored instances
        private readonly ConcurrentDictionary<ServiceDefinition, object> _instances = new ConcurrentDictionary<ServiceDefinition, object>();
        private readonly ConcurrentDictionary<ServiceDefinition, ConcurrentBag<(Type[] types, object obj)>> _genericInstances = new ConcurrentDictionary<ServiceDefinition, ConcurrentBag<(Type[], object)>>();

        private readonly ConcurrentDictionary<Type, (Type type, Delegate factory)> _factoryTypeCache = new ConcurrentDictionary<Type, (Type type, Delegate factory)>();

        public ServiceScope(ServiceResolver serviceResolver, ServiceScope rootScope = null)
        {
            ServiceResolver = serviceResolver;
            RootScope = rootScope;
        }

        public T Get<T>()
        {
            return (T)_Get(typeof(T));
        }

        public T Get<T>(params object[] parameters)
        {
            return (T)_Get(typeof(T), ConstructorParameter.GenerateConstructorParameters(parameters).ToArray());
        }

        public object Get(Type type, params object[] parameters)
        {
            return _Get(type, parameters.Select(p => new ConstructorParameter(p)).ToArray());
        }

        private object _Get(Type type, ConstructorParameter[] parameters = null)
        {
            if (type.IsFactoryValue())
            {
                return CreateFactoryValue(type);
            }

            var services = ServiceResolver.Resolve(type);

            if (services == null || !services.Any())
                return default;

            if (services.Count == 0)
                return default;


            ServiceDefinition service;

            if (services.Count == 1)
            {
                service = services[0];
            }
            else
            {
                int highestMatchScore = 0;
                ServiceDefinition bestServiceDefinition = null;

                foreach (var svc in services.Where(_ => _.Factory == null))
                {
                    var constructorCandidate = BestConstructorCandidate(svc.MatchingConstructors(type), parameters);

                    if ((constructorCandidate?.MatchScore ?? 0) >= highestMatchScore)
                    {
                        highestMatchScore = constructorCandidate?.MatchScore ?? 0;
                        bestServiceDefinition = svc;
                    }
                }

                service = bestServiceDefinition ?? services.Last();
            }

            if (service.RegisteredObject != null)
            {
                if (service.WireProperties || ServiceResolver.WireProperties)
                    SetInjectProperties(service.RegisteredObject);

                return service.RegisteredObject;
            }

            switch (service.Lifetime)
            {
                case ServiceLifetime.Singleton:
                case ServiceLifetime.Scoped:
                    return GetOrAddToScope(service, type, parameters);
                case ServiceLifetime.Transient:
                    return _Get(type, service, parameters);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private object GetOrAddToScope(ServiceDefinition serviceDefinition, Type type, ConstructorParameter[] parameters)
        {
            object instance = null;

            ServiceScope scope = serviceDefinition.Lifetime == ServiceLifetime.Scoped ? this : (RootScope ?? this);

            if (serviceDefinition.IsOpenGenericType)
            {
                var genericTypes = type.GetGenericArguments();
                var bag = scope._genericInstances.GetOrAdd(serviceDefinition, definition => new ConcurrentBag<(Type[], object)>());

                foreach (var entry in bag)
                {
                    if (entry.types.Length != genericTypes.Length)
                        continue;
                    
                    for (var i = 0; i < entry.types.Length; i++)
                    {
                        var itemType = entry.types[i];

                        if (itemType == genericTypes[i])
                        {
                            return entry.obj;
                        }
                    }
                }

                instance = _Get(type, serviceDefinition, parameters);

                bag.Add((genericTypes, instance));
            }
            else
            {
                instance = scope._instances.GetOrAdd(serviceDefinition, svc => _Get(type, svc, parameters));
            }

            foreach (var action in serviceDefinition.AfterResolveActions)
            {
                action(instance, this);
            }

            return instance;
        }

        private object _Get(Type type, ServiceDefinition serviceDefinition, ConstructorParameter[] parameters = null)
        {
            object obj = null;

            if (serviceDefinition.Factory != null)
            {
                obj = serviceDefinition.Factory(this, type);
            }
            else
            {
                obj = CallBestConstructor(serviceDefinition.MatchingConstructors(type), parameters);
            }

            if (obj != null)
            {
                if (serviceDefinition.WireProperties || ServiceResolver.WireProperties)
                    SetInjectProperties(obj);

                foreach (var action in serviceDefinition.AfterCreateActions)
                {
                    action(obj, this);
                }

                return obj;
            }

            return null;
        }

        private object _Create(Type type, ConstructorParameter[] parameters = null)
        {
            var constructors = new ServiceDefinition(type).Constructors;

            var bestConstructor = BestConstructorCandidate(constructors, parameters);

            if (bestConstructor == null)
                return null;

            var obj = bestConstructor.Invoke();

            if (obj == null)
                return null;

            if (ServiceResolver.WireProperties)
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
            return (T)Create(typeof(T), parameters);
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

            foreach (var property in type.GetInjectProperties())
            {
                if (ServiceResolver.CanResolve(property.PropertyType))
                {
                    property.SetValue(o, Get(property.PropertyType));
                }
                else if (property.PropertyType.IsFactoryValue())
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

                var getMethod = typeof(ServiceScope).GetMethod("Get", new[] { typeof(Type), typeof(object[]) });

                var methodCall = Expression.Call(Expression.Constant(this), getMethod, Expression.Constant(targetType), Expression.Constant(new object[0]));

                var lambda = Expression.Lambda(Expression.TypeAs(methodCall, targetType)).Compile();

                return (type.GetGenericTypeDefinition().MakeGenericType(targetType), lambda);
            });

            return Activator.CreateInstance(lazyType, factory);
        }

        private object CallBestConstructor(IEnumerable<ConstructorInfo> constructors, ConstructorParameter[] parameters)
        {
            var bestCandidate = BestConstructorCandidate(constructors, parameters);
        
            return bestCandidate?.Invoke();
        }

        private ConstructorCandidate BestConstructorCandidate(IEnumerable<ConstructorInfo> constructors, ConstructorParameter[] parameters)
        {
            parameters ??= Array.Empty<ConstructorParameter>();

            var bestCandidate = constructors
                .Select(constructor => new ConstructorCandidate(this, constructor, parameters))
                .Where(candidate => candidate.MatchScore >= 0)
                .OrderByDescending(c => c.MatchScore)
                .FirstOrDefault();

            return bestCandidate;
        }

        public bool CanResolve(Type type) => ServiceResolver.CanResolve(type);

        public IServiceScope CreateScope()
        {
            return new ServiceScope(ServiceResolver, RootScope ?? this);
        }

        public void Dispose()
        {
            foreach (var disposable in _instances.Values.OfType<IDisposable>())
            {
                disposable.Dispose();
            }
            foreach (var disposable in _genericInstances.Values.SelectMany(_ => _.Select(__ => __.obj)).OfType<IDisposable>())
            {
                disposable.Dispose();
            }
        }
    }
}