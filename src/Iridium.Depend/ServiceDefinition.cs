using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace Iridium.Depend
{
    internal class ServiceDefinition
    {
        public ServiceDefinition(Type registrationType, object obj = null, Func<IServiceRepository, Type, object> factoryMethod = null)
        {
            RegistrationType = registrationType;
            Type = obj == null ? registrationType : obj.GetType();
            Object = obj;
            Singleton = obj != null;
            Factory = factoryMethod;

            IsUnboundGenericType = RegistrationType.IsGenericTypeDefinition;

            if (!IsUnboundGenericType)
            {
                Constructors = (from c in Type.GetConstructors()
                    let paramCount = c.GetParameters().Length
                    orderby paramCount descending
                    select c).ToArray();
            }
        }

        public readonly Type Type;
        public Type RegistrationType;
        public object Object;
        public bool Singleton;
        public readonly bool IsUnboundGenericType;
        public readonly ConstructorInfo[] Constructors;
        public Func<IServiceRepository, Type, object> Factory;

        public bool IsMatch(Type type)
        {
            if (IsUnboundGenericType)
            {
                if (type.IsGenericTypeDefinition) // type is unbound as well
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

            return _matchingConstructorsCache.GetOrAdd(type, t => (from c in Type.MakeGenericType(type.GenericTypeArguments).GetConstructors()
                let paramCount = c.GetParameters().Length
                orderby paramCount descending
                select c).ToArray());
        }

        private ConcurrentDictionary<Type, ConstructorInfo[]> _matchingConstructorsCache = new ConcurrentDictionary<Type, ConstructorInfo[]>();
    }

    internal class ServiceConstructor
    {
        private ConstructorInfo _constructor;

    }
}