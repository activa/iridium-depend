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
            SingletonObject = obj;
            Singleton = obj != null;
            CreatedFromObject = obj != null;
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


        public object GetSingletonObject(Type type)
        {
            if (IsUnboundGenericType)
            {
                if (GenericSingletonObjects.TryGetValue(type, out var obj))
                    return obj;
            }
            else
            {
                return SingletonObject;
            }

            return null;
        }

        public void SetSingletonObject(Type type, object obj)
        {
            if (IsUnboundGenericType)
            {
                GenericSingletonObjects[type] = obj;
            }
            else
            {
                SingletonObject = obj;
            }
        }

        public readonly Type Type;
        public bool CreatedFromObject;
        public Type RegistrationType;
        public object SingletonObject;
        public bool Singleton;
        public readonly bool IsUnboundGenericType;
        public readonly ConstructorInfo[] Constructors;
        public Func<IServiceRepository, Type, object> Factory;
        public ConcurrentDictionary<Type, object> GenericSingletonObjects = new ConcurrentDictionary<Type, object>();

        public void ClearStoredSingleton(bool dispose)
        {
            if (CreatedFromObject)
                return;

            if (IsUnboundGenericType)
            {
                if (dispose)
                {
                    foreach (var o in GenericSingletonObjects.Values)
                    {
                        if (o is IDisposable disposable)
                            disposable.Dispose();
                    }
                }

                GenericSingletonObjects.Clear();
            }
            else
            {
                if (dispose && SingletonObject is IDisposable disposable) 
                    disposable.Dispose();

                SingletonObject = null;
            }
        }

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

        private readonly ConcurrentDictionary<Type, ConstructorInfo[]> _matchingConstructorsCache = new ConcurrentDictionary<Type, ConstructorInfo[]>();
    }
}