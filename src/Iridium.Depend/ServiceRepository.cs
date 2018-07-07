#region License
//=============================================================================
// Iridium-Core - Portable .NET Productivity Library 
//
// Copyright (c) 2008-2017 Philippe Leybaert
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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Iridium.Reflection;

namespace Iridium.Depend
{
    public class ServiceRepository
    {
        public static ServiceRepository Default = new ServiceRepository();

        private class ServiceDefinition
        {
            public ServiceDefinition(Type type)
            {
                RegistrationType = type;
                Type = type;
                Singleton = false;

                IsUnboundGenericType = Type.GetTypeInfo().IsGenericTypeDefinition;

                if (!IsUnboundGenericType)
                {
                    Constructors = (from c in Type.Inspector().GetConstructors()
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

                    return type.GetGenericTypeDefinition().Inspector().IsAssignableFrom(Type);
                }

                return type.Inspector().IsAssignableFrom(RegistrationType);
            }

            public ConstructorInfo[] MatchingConstructors(Type type)
            {
                if (!IsUnboundGenericType)
                    return Constructors;

                if (!type.IsConstructedGenericType)
                    return new ConstructorInfo[0];

                return (from c in Type.MakeGenericType(type.GenericTypeArguments).Inspector().GetConstructors()
                        let paramCount = c.GetParameters().Length
                        orderby paramCount descending
                        select c).ToArray();
            }
        }

        private readonly List<ServiceDefinition> _services = new List<ServiceDefinition>();

        public T Get<T>()
        {
            return (T)Get(typeof(T));
        }

        private bool CanResolve(Type type)
        {
            return _services.Any(service => service.IsMatch(type));
        }

        public object Get(Type type)
        {
            foreach (var service in _services.Where(svc => svc.IsMatch(type)))
            {
                object obj = service.Object;

                if (obj != null)
                    return obj;

                var constructor = service.MatchingConstructors(type).FirstOrDefault(c => c.GetParameters().All(p => CanResolve(p.ParameterType)));

                if (constructor != null)
                {
                    obj = constructor.Invoke(constructor.GetParameters().Select(p => Get(p.ParameterType)).ToArray());

                    if (service.Singleton)
                        service.Object = obj;

                    return obj;
                }
            }

            return null;
        }

        public object Create(Type type)
        {
            var constructor = new ServiceDefinition(type).Constructors.FirstOrDefault(c => c.GetParameters().All(p => CanResolve(p.ParameterType)));

            if (constructor == null)
                return null;

            return constructor.Invoke(constructor.GetParameters().Select(p => Get(p.ParameterType)).ToArray());
        }

        public T Create<T>() where T : class
        {
            return (T)Create(typeof(T));
        }

        public void UnRegister<T>()
        {
            UnRegister(typeof(T));
        }

        public void UnRegister(Type type)
        {
            _services.RemoveAll(svc => svc.IsMatch(type));
        }

        public IServiceRegistrationResult Register<T>()
        {
            var serviceDefinition = new ServiceDefinition(typeof(T));

            _services.Add(serviceDefinition);

            return new RegistrationResult(this, serviceDefinition);
        }

        public IServiceRegistrationResult Register(Type type)
        {
            var serviceDefinition = new ServiceDefinition(type);

            _services.Add(serviceDefinition);

            return new RegistrationResult(this, serviceDefinition);
        }

        public IServiceRegistrationResult Register<T>(T service)
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));

            var serviceDefinition = new ServiceDefinition(typeof(T), service);

            _services.Add(serviceDefinition);

            return new RegistrationResult(this, serviceDefinition);
        }

        public Type[] RegisteredTypes()
        {
            return _services.Select(svc => svc.RegistrationType).ToArray();
        }

        private void RemoveConflicting(Type registrationType, ServiceDefinition svcDef)
        {
            _services.RemoveAll(svc => !ReferenceEquals(svcDef, svc) && svc.IsMatch(registrationType));
        }

        private class RegistrationResult : IServiceRegistrationResult
        {
            private ServiceDefinition Svc { get; }
            private ServiceRepository Repository { get; }

            public RegistrationResult(ServiceRepository repo, ServiceDefinition svc)
            {
                Repository = repo;
                Svc = svc;
            }

            public IServiceRegistrationResult As<T>()
            {
                return As(typeof(T));
            }

            public IServiceRegistrationResult As(Type type)
            {
                if (!Svc.IsUnboundGenericType && !type.GetTypeInfo().IsAssignableFrom(Svc.Type.GetTypeInfo()))
                    throw new ArgumentException("Type is not compatible");

                Svc.RegistrationType = type;

                return this;
            }

            public IServiceRegistrationResult Singleton()
            {
                Svc.Singleton = true;

                return this;
            }

            public IServiceRegistrationResult Replace<T>()
            {
                Repository.RemoveConflicting(typeof(T), Svc);

                return this;
            }

            public IServiceRegistrationResult Replace(Type type)
            {
                Repository.RemoveConflicting(type, Svc);

                return this;
            }

            public Type RegisteredAsType => Svc.RegistrationType;
        }
    }
}