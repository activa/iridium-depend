#region License
//=============================================================================
// Iridium-Depend - Portable .NET Productivity Library 
//
// Copyright (c) 2008-2022 Philippe Leybaert
//
// Permission is hereby granted, free of charge, to any person obtaining a copy 
// of this software and associated documentation files (the "Software"), to deal 
// in the Software without restriction, including without limitation the rights 
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
// copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in 
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// IN THE SOFTWARE.
//=============================================================================
#endregion

using System;
using System.Linq;
using System.Reflection;

namespace Iridium.Depend
{
    internal class ConstructorCandidate
    {
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
                            if (namedParameter.Type.IsAssignableTo(constructorParameterType))
                            {
                                _parameterValues[i] = namedParameter;

                                resolvedParametersCount++;
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < numConstructorParameters; i++)
            {
                if (_parameterValues[i] != null)
                    continue;

                var parameterType = _constructorParameters[i].ParameterType;

                if (_constructorParameters[i].GetCustomAttribute<ParameterAttribute>() == null && (serviceResolver.CanResolve(parameterType) || parameterType.IsLazyType() || parameterType.IsDeferredListType()))
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

            MatchScore = 100 * (100 + (resolvedParametersCount + resolvedServicesCount) - numConstructorParameters) + numConstructorParameters;
        }

        public object Invoke(ServiceProvider serviceProvider)
        {
            for (int i = 0; i < _constructorParameters.Length; i++)
            {
                if (_parameterValues[i] != null)
                    continue;

                _parameterValues[i] = new ConstructorParameter(value:null);
            }

            return _constructor.Invoke(_parameterValues.Select(p => p.Value(serviceProvider)).ToArray());
        }
    }
}