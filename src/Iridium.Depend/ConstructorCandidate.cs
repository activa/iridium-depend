using System;
using System.Collections.Concurrent;
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

        public ConstructorCandidate(ServiceResolver serviceResolver, ConstructorInfo constructor, ConstructorParameter[] parameters)
        {
            _constructor = constructor;
            _constructorParameters = _constructor.GetParameters();

            if (parameters is { Length: 0 })
                parameters = null;

            var numConstructorParameters = _constructorParameters.Length;

            if (parameters != null && numConstructorParameters < parameters.Length)
            {
                MatchScore = -1;
                return;
            }

            int resolvedServicesCount = 0;
            int resolvedParametersCount = 0;

            _parameterValues = new ConstructorParameter[numConstructorParameters];

            if (parameters != null)
            {
                var namedParameters = parameters.Where(p => p.Name != null).ToDictionary(p => p.Name, p => p);

                if (namedParameters.Count > 0)
                {
                    for (int i = 0; i < numConstructorParameters; i++)
                    {
                        var constructorParameter = _constructorParameters[i];
                        var constructorParameterType = constructorParameter.ParameterType;

                        if (namedParameters.TryGetValue(constructorParameter.Name, out var namedParameter))
                        {
                            if (!namedParameter.Type.IsAssignableTo(constructorParameterType))
                                throw new Exception($"Parameter {namedParameter.Name} can't be assigned to type {constructorParameterType.Name}");

                            _parameterValues[i] = namedParameter;

                            resolvedParametersCount++;
                        }
                    }
                }
            }

            for (int i = 0; i < numConstructorParameters; i++)
            {
                if (_parameterValues[i] != null)
                    continue;

                var parameterType = _constructorParameters[i].ParameterType;

                if (serviceResolver.CanResolve(parameterType) || (parameterType.IsDeferredType() && serviceResolver.CanResolve(parameterType.DeferredType())))
                {
                   _parameterValues[i] = new ConstructorParameter(serviceProvider => serviceProvider.Resolve(parameterType));
                   resolvedServicesCount++;
                }
            }

            if (parameters != null)
            {
                var typedParameters = parameters.Where(p => p.Name == null && p.Type != null).ToList();
                var numNullParameters = parameters.Count(p => p.Name == null && p.Type == null);

                for (int i = 0; i < numConstructorParameters; i++)
                {
                    if (_parameterValues[i] != null)
                        continue;

                    var constructorParameter = _constructorParameters[i];
                    var constructorParameterType = constructorParameter.ParameterType;

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

                if (resolvedParametersCount < parameters.Length)
                {
                    MatchScore = -1;
                    return;
                }
            }

            var resolveCount = resolvedParametersCount + resolvedServicesCount;

            MatchScore = 100 * (100 + resolveCount - numConstructorParameters) + numConstructorParameters;
        }


        private static ConcurrentDictionary<TypeCollection, Delegate> _constructorDelegates = new ConcurrentDictionary<TypeCollection, Delegate>();


        public object Invoke(ServiceProvider serviceProvider)
        {
            for (int i = 0; i < _constructorParameters.Length; i++)
            {
                if (_parameterValues[i] != null)
                    continue;

                _parameterValues[i] = new ConstructorParameter(value:null);
            }

            // var typeCollection = new TypeCollection(_constructorParameters.Select(_ => _.ParameterType).Append(_constructor.DeclaringType).ToArray());
            //
            // var method = _constructorDelegates.GetOrAdd(typeCollection, info => CreateDelegate());
            //
            // return method.DynamicInvoke(_parameterValues.Select(p => p.Value).ToArray());

            return _constructor.Invoke(_parameterValues.Select(p => p.Value(serviceProvider)).ToArray());
        }

        private Delegate CreateDelegate()
        {
            var expressionParameters = _constructorParameters.Select(_ => Expression.Parameter(_.ParameterType, _.Name)).ToArray();

            var method = Expression.Lambda(
                //delegateType,
                Expression.New(_constructor, expressionParameters),
                expressionParameters
                //delegateArgs.Select(Expression.Parameter)
                //                _constructorParameters.Select(_ => Expression.Parameter(_.ParameterType))

            ).Compile();

            return method;
        }
    }
}