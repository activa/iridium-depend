using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Iridium.Depend
{
    internal class ConstructorCandidate
    {
        private readonly ServiceRepository _repo;
        private readonly ConstructorInfo _constructor;
        private readonly ParameterInfo[] _constructorParameters;
        private readonly ConstructorParameter[] _parameterValues;

        public int MatchScore { get; }

        public ConstructorCandidate(ServiceRepository repo, ConstructorInfo constructor, ConstructorParameter[] parameters)
        {
            _repo = repo;
            _constructor = constructor;
            _constructorParameters = _constructor.GetParameters();

            var numConstructorParameters = _constructorParameters.Length;

            if (numConstructorParameters < parameters.Length)
            {
                MatchScore = -1;
                return;
            }

            _parameterValues = new ConstructorParameter[numConstructorParameters];

            int resolvedParametersCount = 0;

            if (parameters.Length > 0)
            {
                var typedParameters = parameters.Where(p => p.Name == null).ToList();
                var namedParameters = parameters.Where(p => p.Name != null).ToDictionary(p => p.Name, p => p);

                for (int i = 0; i < numConstructorParameters; i++)
                {
                    if (namedParameters.TryGetValue(_constructorParameters[i].Name, out var namedParameter))
                    {
                        if (!namedParameter.Type.IsAssignableTo(_constructorParameters[i].ParameterType))
                            throw new Exception($"Parameter {namedParameter.Name} can't be assigned to type {_constructorParameters[i].ParameterType.Name}");

                        _parameterValues[i] = namedParameter;
                        resolvedParametersCount++;
                    }
                    else
                    {
                        foreach (var parameter in typedParameters)
                        {
                            if (parameter.Type.IsAssignableTo(_constructorParameters[i].ParameterType))
                            {
                                _parameterValues[i] = parameter;
                                resolvedParametersCount++;
                                break;
                            }
                        }
                    }
                }

                if (resolvedParametersCount < parameters.Length)
                {
                    MatchScore = -1;
                    return;
                }
            }

            var resolveCount = resolvedParametersCount + _constructorParameters.Where((t, i) => _parameterValues[i] == null).Count(t => repo.CanResolve(t.ParameterType));

            if (resolveCount == numConstructorParameters)
                MatchScore = int.MaxValue;
            else
                MatchScore = 100 * (100 + resolveCount - numConstructorParameters) + numConstructorParameters;
        }

        public object Invoke()
        {
            for (int i = 0; i < _constructorParameters.Length; i++)
            {
                _parameterValues[i] ??= new ConstructorParameter(_repo.Get(_constructorParameters[i].ParameterType));
            }

            return _constructor.Invoke(_parameterValues.Select(p => p.Value).ToArray());
        }
    }
}