using System;
using System.Collections.Generic;
using System.Linq;

namespace Iridium.Depend
{
    public class ValueWithType
    {
        public object Value;
        public Type Type;

        public ValueWithType(object value, Type type)
        {
            Value = value;
            Type = type;
        }

        public ValueWithType(object value)
        {
            Value = value;
            Type = value?.GetType() ?? typeof(object);
        }
    }

    public class ValueWithType<T> : ValueWithType
    {
        public ValueWithType(T value) : base(value, typeof(T))
        {
        }
    }

}