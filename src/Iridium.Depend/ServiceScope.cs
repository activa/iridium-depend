using System;
using System.Collections.Concurrent;
using System.Data.Common;
using System.Linq;

namespace Iridium.Depend
{
    internal class ServiceScope
    {
        private readonly ConcurrentDictionary<ServiceDefinition, object> _instances = new ConcurrentDictionary<ServiceDefinition, object>();
        private readonly ConcurrentDictionary<(ServiceDefinition serviceDefinition, TypeCollection typeArgs), object> _genericInstances = new ConcurrentDictionary<(ServiceDefinition serviceDefinition, TypeCollection typeArgs), object>();

        public ServiceScope()
        {
        }

        public object GetOrStore(ServiceDefinition service, Type type, Func<Type,ServiceDefinition,ConstructorParameter[],object> factory, ConstructorParameter[] parameters = null)
        {
            object instance;

            if (service.IsOpenGenericType)
            {
                var typeArgs = new TypeCollection(type.GetGenericArguments());

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

            return instance;
        }

        public void Dispose()
        {
            foreach (var disposable in _instances.Where(svc => !svc.Key.SkipDispose).Select(svc => svc.Value).OfType<IDisposable>())
            {
                disposable.Dispose();
            }

            foreach (var disposable in _genericInstances.Where(svc => !svc.Key.serviceDefinition.SkipDispose).Select(svc => svc.Value).OfType<IDisposable>())
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