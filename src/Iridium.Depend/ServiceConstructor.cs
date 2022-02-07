using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace Iridium.Depend
{
    internal class ServiceConstructor
    {
        private readonly ConcurrentDictionary<Type, ConstructorInfo> _genericConstructorsCache = new ConcurrentDictionary<Type, ConstructorInfo>();

        private readonly ServiceDefinition _serviceDefinition;
        private readonly ConstructorInfo _constructorInfo;
        private ConstructorCandidate _constructorCandidate;

        public ServiceConstructor(ServiceDefinition serviceDefinition, ConstructorInfo constructorInfo)
        {
            _serviceDefinition = serviceDefinition;
            _constructorInfo = constructorInfo;
        }

        public ConstructorInfo Constructor(Type type)
        {
            if (!_serviceDefinition.IsOpenGenericType)
                return _constructorInfo;

            if (!type.IsConstructedGenericType)
                throw new Exception("Can't create object from open generic type");

            return _genericConstructorsCache.GetOrAdd(
                type,
                t => _serviceDefinition.Type
                    .MakeGenericType(t.GenericTypeArguments)
                    .GetConstructors()
                    .FirstOrDefault(c => c.MetadataToken == _constructorInfo.MetadataToken)
                );
        }

        public ConstructorCandidate ConstructorCandidate => _constructorCandidate;

        public void PreResolve(ServiceResolver serviceResolver)
        {
            _constructorCandidate = new ConstructorCandidate(serviceResolver, _constructorInfo, null);
        }
    }
}