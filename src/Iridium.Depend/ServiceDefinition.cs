using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Iridium.Depend
{
    internal class ServiceDefinition
    {
        public ServiceDefinition(Type type)
        {
            RegistrationType = type;
            Type = type;
            Singleton = false;

            IsUnboundGenericType = Type.GetTypeInfo().IsGenericTypeDefinition;

            if (!IsUnboundGenericType)
            {
                Constructors = (from c in Type.GetConstructors()
                    let paramCount = c.GetParameters().Length
                    orderby paramCount descending
                    select c).ToArray();
            }
        }

        public ServiceDefinition(Type registrationType, object obj)
        {
            RegistrationType = registrationType;
            Type = obj.GetType();
            Object = obj;
            Singleton = true;
        }

        public readonly Type Type;
        public Type RegistrationType;
        public object Object;
        public bool Singleton;
        public readonly bool IsUnboundGenericType;
        public readonly ConstructorInfo[] Constructors;

        public bool IsMatch(Type type)
        {
            if (IsUnboundGenericType)
            {
                if (type.GetTypeInfo().IsGenericTypeDefinition) // type is unbound as well
                    return type == RegistrationType;

                if (!type.IsConstructedGenericType)
                {
                    return false;
                }

                return Type.IsAssignableTo(type.GetGenericTypeDefinition());
            }

            return RegistrationType.IsAssignableTo(type);
        }

        public ConstructorInfo[] MatchingConstructors(Type type)
        {
            if (!IsUnboundGenericType)
                return Constructors;

            if (!type.IsConstructedGenericType)
                return new ConstructorInfo[0];

            return (from c in Type.MakeGenericType(type.GenericTypeArguments).GetConstructors()
                let paramCount = c.GetParameters().Length
                orderby paramCount descending
                select c).ToArray();
        }
    }
}