using System;
using System.Collections.Concurrent;
using System.Data.Common;
using System.Linq;

namespace Iridium.Depend
{
    internal class ServiceScope
    {
        private bool _allowMultipleSingletonCreations = true;

        // stored instances

        private readonly ConcurrentDictionary<ServiceDefinition, object> _instances = new ConcurrentDictionary<ServiceDefinition, object>();
        private readonly ConcurrentDictionary<(ServiceDefinition serviceDefinition, TypeCollection typeArgs), object> _genericInstances = new ConcurrentDictionary<(ServiceDefinition serviceDefinition, TypeCollection typeArgs), object>();

        public ServiceScope(bool allowMultipleSingletonCreations = true)
        {
            _allowMultipleSingletonCreations = allowMultipleSingletonCreations;
        }

        public ServiceScope(ServiceScope parentScope)
        {
            _allowMultipleSingletonCreations = parentScope._allowMultipleSingletonCreations;
        }

        public object GetOrStore(ServiceDefinition service, Type type, Func<Type,ServiceDefinition,ConstructorParameter[],object> factory, ConstructorParameter[] parameters = null)
        {
            object instance;

            if (service.IsOpenGenericType)
            {
                var typeArgs = new TypeCollection(type.GetGenericArguments());

                if (_allowMultipleSingletonCreations)
                {
                    instance = _genericInstances.GetOrAdd((service, typeArgs), svc => factory(type, svc.serviceDefinition, parameters));
                }
                else
                {
                    if (!_genericInstances.TryGetValue((service, typeArgs), out instance))
                    {
                        lock (_genericInstances)
                        {
                            if (!_genericInstances.TryGetValue((service, typeArgs), out instance))
                            {
                                instance = factory(type, service, parameters);

                                _genericInstances.TryAdd((service, typeArgs), instance);
                            }
                        }
                    }
                }
            }
            else
            {
                if (_allowMultipleSingletonCreations)
                {
                    instance = _instances.GetOrAdd(service, svc => factory(type, svc, parameters));
                }
                else
                {
                    if (!_instances.TryGetValue(service, out instance))
                    {
                        lock (_instances)
                        {
                            if (!_instances.TryGetValue(service, out instance))
                            {
                                instance = factory(type, service, parameters);

                                _instances.TryAdd(service, instance);
                            }
                        }
                    }
                }
            }

            return instance;
        }

        public void Dispose()
        {
            foreach (var disposable in _instances.Values.OfType<IDisposable>())
            {
                disposable.Dispose();
            }
            foreach (var disposable in _genericInstances.Values.OfType<IDisposable>())
            {
                disposable.Dispose();
            }
        }
    }

    internal class ServiceFactory
    {
        private readonly ConcurrentDictionary<Type, (Type type, Delegate factory)> _factoryTypeCache = new ConcurrentDictionary<Type, (Type type, Delegate factory)>();
        private readonly ServiceDefinition _serviceDefinition;
        private readonly ServiceScope _serviceScope;

        public ServiceFactory(ServiceDefinition serviceDefinition, ServiceScope serviceScope)
        {
            _serviceDefinition = serviceDefinition;
            _serviceScope = serviceScope;
        }
    }
}