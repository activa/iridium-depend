using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Iridium.Depend
{
    internal class ServiceDefinition
    {
        private readonly ConcurrentDictionary<Type, ConstructorInfo[]> _genericConstructorsCache = new ConcurrentDictionary<Type, ConstructorInfo[]>();
        private List<Type> _registrationTypes;
        private readonly ConstructorInfo[] _constructors;
        // private readonly ServiceConstructor[] _serviceConstructors;

        public IEnumerable<Type> RegistrationTypes => _registrationTypes ?? GenerateDefaultRegistrationTypes();

        public void AddRegistrationType(Type type)
        {
            _registrationTypes ??= new List<Type>();

            _registrationTypes.Add(type);
        }

        public readonly Type Type;
        public ServiceLifetime Lifetime;
        public readonly bool IsOpenGenericType;
        public Func<IServiceProvider, Type, object> Factory;
        public readonly object RegisteredObject;
        public bool SkipDispose;
        //public bool WireProperties;
        public List<Action<object, IServiceProvider>> AfterCreateActions = new List<Action<object, IServiceProvider>>();
        public List<Action<object, IServiceProvider>> AfterResolveActions = new List<Action<object, IServiceProvider>>();

        public ServiceDefinition(Type type, object obj = null, ServiceLifetime lifetime = ServiceLifetime.Transient, Func<IServiceProvider, Type, object> factoryMethod = null)
        {
            RegisteredObject = obj;
            Type = type;
            Lifetime = lifetime;
            Factory = factoryMethod;

            if (obj != null)
                SkipDispose = true;

            IsOpenGenericType = type.IsGenericTypeDefinition;

            if (!IsOpenGenericType)
            {
                _constructors = Type.GetConstructors().ToArray();
            }

            // _serviceConstructors = (from c in Type.GetConstructors()
            //     select new ServiceConstructor(type, c)).ToArray();
        }

        private List<Type> GenerateDefaultRegistrationTypes()
        {
            var registrationTypes = Type.GetInterfaces().AsEnumerable();

            if (IsOpenGenericType)
            {
                registrationTypes = registrationTypes.Select(_ => _.GetGenericTypeDefinition());
            }

            return registrationTypes.Append(Type).Concat(GetAllPublicBaseTypes(Type)).ToList();
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

        public ConstructorInfo[] Constructors(Type type)
        {
            if (!IsOpenGenericType)
                return _constructors;

            if (!type.IsConstructedGenericType)
                throw new Exception("Can't create object from open generic type");

            return _genericConstructorsCache.GetOrAdd(type, t => Type.MakeGenericType(type.GenericTypeArguments).GetConstructors().ToArray());
        }

        //public ServiceConstructor[] ServiceConstructors => _serviceConstructors;
    }
}