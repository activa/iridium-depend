using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Iridium.Depend
{
    internal class ServiceDefinition
    {
        private List<Type> _registrationTypes = null;

        public IEnumerable<Type> RegistrationTypes => _registrationTypes ?? GenerateDefaultRegistrationTypes();

        public void AddRegistrationType(Type type)
        {
            _registrationTypes ??= new List<Type>();

            _registrationTypes.Add(type);
        }

        public readonly Type Type;
        public ServiceLifetime Lifetime;
        public readonly bool IsOpenGenericType;
        public readonly ConstructorInfo[] Constructors;
        public Func<IServiceProvider, Type, object> Factory;
        public readonly object RegisteredObject;
        public bool WireProperties;
        public List<Action<object, IServiceProvider>> AfterCreateActions = new List<Action<object, IServiceProvider>>();
        public List<Action<object, IServiceProvider>> AfterResolveActions = new List<Action<object, IServiceProvider>>();

        public ServiceDefinition(Type type, object obj = null, ServiceLifetime lifetime = ServiceLifetime.Transient, Func<IServiceProvider, Type, object> factoryMethod = null)
        {
            RegisteredObject = obj;
            Type = type;
            Lifetime = lifetime;
            Factory = factoryMethod;

            IsOpenGenericType = type.IsGenericTypeDefinition;

            if (!IsOpenGenericType)
            {
                Constructors = (from c in Type.GetConstructors()
                    let paramCount = c.GetParameters().Length
                    orderby paramCount descending
                    select c).ToArray();
            }
        }

        private List<Type> GenerateDefaultRegistrationTypes()
        {
            if (IsOpenGenericType)
            {
                return Type.GetInterfaces().Select(_ => _.GetGenericTypeDefinition()).Append(Type).Concat(GetAllPublicBaseTypes(Type)).ToList();
            }
            else
            {
                return Type.GetInterfaces().Append(Type).Concat(GetAllPublicBaseTypes(Type)).ToList();
            }

        }

        private static IEnumerable<Type> GetAllPublicBaseTypes(Type type)
        {
            var baseType = type.BaseType;

            while (baseType != null && baseType != typeof(object) && baseType.IsPublic)
            {
                yield return baseType;

                baseType = baseType.BaseType;
            }
        }

        public ConstructorInfo[] MatchingConstructors(Type type)
        {
            if (!IsOpenGenericType)
                return Constructors;

            if (!type.IsConstructedGenericType)
                return Array.Empty<ConstructorInfo>();

            return _matchingConstructorsCache.GetOrAdd(type, t => (from c in Type.MakeGenericType(type.GenericTypeArguments).GetConstructors()
                let paramCount = c.GetParameters().Length
                orderby paramCount descending
                select c).ToArray());
        }

        private readonly ConcurrentDictionary<Type, ConstructorInfo[]> _matchingConstructorsCache = new ConcurrentDictionary<Type, ConstructorInfo[]>();

    }
}