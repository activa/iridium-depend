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

        public bool WireProperties { get; }

        public ServiceResolver(ServiceRepository repo, bool wireProperties = false)
        {
            RebuildServiceMap(repo);

            WireProperties = wireProperties;

            repo.Changed += (sender, args) => { RebuildServiceMap((ServiceRepository)sender); };
        }

        private void RebuildServiceMap(ServiceRepository repo)
        {
            foreach (var svc in repo.ServiceDefinitions.Where(s => !s.IsOpenGenericType))
            {
                foreach (var type in svc.RegistrationTypes)
                {
                    var list = _serviceMap.GetOrAdd(type, type1 => new List<ServiceDefinition>());

                    list.Add(svc);
                }
            }

            foreach (var svc in repo.ServiceDefinitions.Where(s => s.IsOpenGenericType))
            {
                foreach (var type in svc.RegistrationTypes)
                {
                    var list = _genericServiceMap.GetOrAdd(type, type1 => new List<ServiceDefinition>());

                    list.Add(svc);
                }
            }
        }

        public List<ServiceDefinition> Resolve(Type type)
        {
            if (_serviceMap.TryGetValue(type, out var services))
            {
                return services;
            }

            if (type.IsGenericType)
            {
                if (_genericServiceMap.TryGetValue(type.GetGenericTypeDefinition(), out var genericServices))
                {
                    return genericServices;
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