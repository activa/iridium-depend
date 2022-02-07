using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Iridium.Depend
{
    internal class ServiceResolver
    {
        private readonly ConcurrentDictionary<Type, List<ServiceDefinition>> _serviceMap = new ConcurrentDictionary<Type, List<ServiceDefinition>>();
        private readonly ConcurrentDictionary<Type, List<ServiceDefinition>> _genericServiceMap = new ConcurrentDictionary<Type, List<ServiceDefinition>>();

        public ServiceResolver(ServiceRepository repo)
        {
            RebuildServiceMap(repo);

            repo.Changed += (sender, args) => { RebuildServiceMap((ServiceRepository)sender); };
        }

        private void RebuildServiceMap(ServiceRepository repo)
        {
            var serviceDefinitions = repo.ServiceDefinitions;

            foreach (var svc in serviceDefinitions.Where(s => !s.IsOpenGenericType))
            {
                foreach (var type in svc.RegistrationTypes)
                {
                    var list = _serviceMap.GetOrAdd(type, type1 => new List<ServiceDefinition>());

                    list.Add(svc);
                }
            }

            foreach (var svc in serviceDefinitions.Where(s => s.IsOpenGenericType))
            {
                foreach (var type in svc.RegistrationTypes)
                {
                    var list = _genericServiceMap.GetOrAdd(type, type1 => new List<ServiceDefinition>());

                    list.Add(svc);
                }
            }

            // foreach (var serviceDefinition in repo.ServiceDefinitions)
            // {
            //     foreach (var serviceConstructor in serviceDefinition.ServiceConstructors)
            //     {
            //         serviceConstructor.PreResolveParameters(this);
            //     }
            // }
        }

        public ServiceDefinition Resolve(Type type)
        {
            if (_serviceMap.TryGetValue(type, out var services))
            {
                return services.Last();
            }

            if (type.IsGenericType)
            {
                if (_genericServiceMap.TryGetValue(type.GetGenericTypeDefinition(), out var genericServices))
                {
                    return genericServices.Last();
                }
            }

            return null;
        }

        public ServiceDefinition[] ResolveAll(Type type)
        {
            if (_serviceMap.TryGetValue(type, out var services))
            {
                return services.ToArray();
            }

            if (type.IsGenericType)
            {
                if (_genericServiceMap.TryGetValue(type.GetGenericTypeDefinition(), out var genericServices))
                {
                    return genericServices.ToArray();
                }
            }

            return null;
        }

        public bool CanResolve(Type type)
        {
            if (_serviceMap.ContainsKey(type))
                return true;
            
            if (type.IsGenericType)
            {
                if (_genericServiceMap.ContainsKey(type.GetGenericTypeDefinition()))
                    return true;
            }

            return false;
        }
    }
}