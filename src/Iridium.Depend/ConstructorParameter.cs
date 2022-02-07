using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Iridium.Depend
{
    internal class ConstructorParameter
    {
        public string Name;
        private readonly object _value;
        public Type Type;
        private readonly Func<ServiceProvider, object> _factory;
        //public Type ServiceType;

        // public ConstructorParameter(Type serviceType)
        // {
        //     ServiceType = serviceType;
        // }

        public ConstructorParameter(Func<ServiceProvider,object> factory)
        {
            _factory = factory;
        }

        public ConstructorParameter(string name, object value, Type type)
        {
            Name = name;
            _value = value;
            Type = type;
        }

        public ConstructorParameter(object value)
        {
            _value = value;
            Type = value?.GetType();
        }

        public object Value(ServiceProvider serviceProvider) => _factory != null ? _factory(serviceProvider) : _value;

        

        internal static IEnumerable<ConstructorParameter> GenerateConstructorParameters(object[] parameters)
        {
            foreach (var parameter in parameters)
            {
                if (parameter.IsAnonymousType())
                {
                    foreach (var property in parameter.GetType().GetProperties())
                    {
                        yield return new ConstructorParameter(property.Name, property.GetValue(parameter), property.PropertyType);
                    }
                }
                else
                {
                    yield return new ConstructorParameter(parameter);
                }
            }
        }
    }

}