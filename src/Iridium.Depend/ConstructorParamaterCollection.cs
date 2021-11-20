using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Iridium.Depend
{
    internal class ParamaterCollection
    {
        private readonly List<ConstructorParameter> _parameters;
        private readonly List<ConstructorParameter> _typedParameters;
        private readonly Dictionary<string, ConstructorParameter> _namedParameters;

        public ParamaterCollection(IEnumerable<ConstructorParameter> parameters)
        {
            _parameters = parameters.ToList();
            _typedParameters = _parameters.Where(p => p.Name == null && p.Type != null).ToList();
            _namedParameters = _parameters.Where(p => p.Name != null).ToDictionary(p => p.Name, p => p);
        }

        public int NumNullParameters => _parameters.Count - _typedParameters.Count - _namedParameters.Count;

        public bool ResolveNamedParameter(ParameterInfo constructorParameter, out ConstructorParameter matchingParameter)
        {
            if (_namedParameters.TryGetValue(constructorParameter.Name, out var namedParameter))
            {
                if (!namedParameter.Type.IsAssignableTo(constructorParameter.ParameterType))
                    throw new Exception($"Parameter {namedParameter.Name} can't be assigned to type {constructorParameter.ParameterType.Name}");

                matchingParameter = namedParameter;

                return true;
            }

            matchingParameter = null;

            return false;
        }
    }
}