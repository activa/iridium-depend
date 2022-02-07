using System;
using System.Reflection;

namespace Iridium.Depend
{
    internal class ServiceConstructor
    {
        private readonly ConstructorInfo _constructorInfo;
        private readonly ParameterInfo[] _constructorParameters;
        private readonly ServiceDefinition[] _resolvedServiceParameters;
        private readonly bool _isGenericTypeDefinition;

        public ServiceConstructor(Type type, ConstructorInfo constructorInfo)
        {
            _isGenericTypeDefinition = type.IsGenericTypeDefinition;
            _constructorInfo = constructorInfo;
            _constructorParameters = constructorInfo.GetParameters();
            _resolvedServiceParameters = new ServiceDefinition[_constructorParameters.Length];
        }

        public ConstructorInfo Constructor => _constructorInfo;
        public ParameterInfo[] ConstructorParameters => _constructorParameters;

        public void PreResolveParameters(ServiceResolver serviceResolver)
        {
            for (int i = 0; i < _constructorParameters.Length; i++)
            {
                var param = _constructorParameters[i];
                var paramType = param.ParameterType;

                if (paramType.IsDeferredType())
                    _resolvedServiceParameters[i] = serviceResolver.Resolve(paramType.DeferredType());
                else
                    _resolvedServiceParameters[i] = serviceResolver.Resolve(param.ParameterType);
            }
        }
    }
}