using System;
using System.Collections.Generic;
using System.Linq;

namespace Iridium.Depend
{
    public class ConstructorParameter
    {
        public string Name;
        public object Value;
        public Type Type;

        public ConstructorParameter(string name, object value, Type type)
        {
            Name = name;
            Value = value;
            Type = type;
        }

        public ConstructorParameter(object value, Type type)
        {
            Value = value;
            Type = type;
        }

        public ConstructorParameter(object value)
        {
            Value = value;
            Type = value?.GetType() ?? typeof(object);
        }

        public ConstructorParameter(string name, object value)
        {
            Name = name;
            Value = value;
            Type = value?.GetType() ?? typeof(object);
        }

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

    public class ConstructorParameter<T> : ConstructorParameter
    {
        public ConstructorParameter(string name, T value) : base(name, value, typeof(T))
        {
        }

        public ConstructorParameter(T value) : base(value, typeof(T))
        {
        }
    }

}