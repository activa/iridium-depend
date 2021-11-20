using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Iridium.Depend
{
    internal class ConstructorCandidate
    {
        // private readonly ServiceRepository _repo;
        private readonly ConstructorInfo _constructor;
        private readonly ParameterInfo[] _constructorParameters;
        private readonly ConstructorParameter[] _parameterValues;

        public int MatchScore { get; }

        public ConstructorCandidate(ServiceRepository repo, ConstructorInfo constructor, ConstructorParameter[] parameters)
        {
            _constructor = constructor;
            _constructorParameters = _constructor.GetParameters();

            var numConstructorParameters = _constructorParameters.Length;

            if (numConstructorParameters < parameters.Length)
            {
                MatchScore = -1;
                return;
            }

            int resolvedServicesCount = 0;
            int resolvedParametersCount = 0;

            _parameterValues = new ConstructorParameter[numConstructorParameters];

            for (int i = 0; i < _parameterValues.Length; i++)
            {
                var parameterType = _constructorParameters[i].ParameterType;

                if (repo.CanResolve(parameterType) || parameterType.IsFactoryValue())
                {
                   _parameterValues[i] = new ConstructorParameter(() => repo.Get(parameterType));
                   resolvedServicesCount++;
                }
            }

            if (parameters.Length > 0)
            {
                var typedParameters = parameters.Where(p => p.Name == null && p.Type != null).ToList();
                var numNullParameters = parameters.Count(p => p.Name == null && p.Type == null);
                var namedParameters = parameters.Where(p => p.Name != null).ToDictionary(p => p.Name, p => p);

                for (int i = 0; i < numConstructorParameters; i++)
                {
                    if (_parameterValues[i] != null)
                        continue;

                    var constructorParameter = _constructorParameters[i];
                    var constructorParameterType = constructorParameter.ParameterType;

                    if (namedParameters.TryGetValue(constructorParameter.Name, out var namedParameter))
                    {
                        if (!namedParameter.Type.IsAssignableTo(constructorParameterType))
                            throw new Exception($"Parameter {namedParameter.Name} can't be assigned to type {constructorParameterType.Name}");

                        _parameterValues[i] = namedParameter;
                        resolvedParametersCount++;
                    }
                    else
                    {
                        foreach (var parameter in typedParameters.Where(p => p.Type.IsAssignableTo(constructorParameterType)))
                        {
                            _parameterValues[i] = parameter;
                            resolvedParametersCount++;
                            typedParameters.Remove(parameter);
                            break;
                        }

                        if (_parameterValues[i] == null && numNullParameters > 0 && constructorParameterType.CanBeNull())
                        {
                            _parameterValues[i] = new ConstructorParameter(value:null);
                            numNullParameters--;
                            resolvedParametersCount++;
                        }
                    }
                }

                if (resolvedParametersCount < parameters.Length)
                {
                    MatchScore = -1;
                    return;
                }
            }

            var resolveCount = resolvedParametersCount + resolvedServicesCount;

            MatchScore = 100 * (100 + resolveCount - numConstructorParameters) + numConstructorParameters;
        }


        public object Invoke()
        {
            for (int i = 0; i < _constructorParameters.Length; i++)
            {
                if (_parameterValues[i] != null)
                    continue;

                _parameterValues[i] = new ConstructorParameter(value:null);
            }

            return _constructor.Invoke(_parameterValues.Select(p => p.Value).ToArray());
        }
    }
}