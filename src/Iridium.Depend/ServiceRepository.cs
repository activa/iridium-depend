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
using System.ComponentModel;
using System.Linq;
using System.Reflection;
//using System.Reflection;
//using BindingFlags = System.Reflection.BindingFlags;

namespace Iridium.Depend
{
    public interface IServiceRepository
    {
    }

    public class ServiceRepository : IServiceRepository
    {
        public static ServiceRepository Default = new ServiceRepository();

        private readonly List<ServiceDefinition> _services = new List<ServiceDefinition>();
        private readonly ServiceRepository _parent;
        private readonly Dictionary<Type, PropertyInfo[]> _injectProperties = new Dictionary<Type, PropertyInfo[]>();

        public ServiceRepository()
        {
        }

        public ServiceRepository(ServiceRepository parent) => _parent = parent;

        public ServiceRepository CreateChild() => new ServiceRepository(this);

        public T Get<T>()
        {
            return (T) _Get(typeof(T));
        }

        public T Get<T>(params object[] parameters)
        {
            return (T) _Get(typeof(T), parameters.Select(p => new ValueWithType(p)).ToArray());
        }

        public T Get<T, TParam1>(TParam1 param)
        {
            return (T) _Get(typeof(T), new ValueWithType[] {new ValueWithType<TParam1>(param)});
        }

        public T Get<T, TParam1, TParam2>(TParam1 param1, TParam2 param2)
        {
            return (T)_Get(typeof(T), new ValueWithType[] { new ValueWithType<TParam1>(param1), new ValueWithType<TParam2>(param2) });
        }

        private bool CanResolve(Type type)
        {
            return Services.Any(service => service.IsMatch(type));
        }

        private IEnumerable<ServiceDefinition> Services
        {
            get
            {
                if (_parent == null)
                    return _services;

                return _parent.Services.Union(_services);
            }
        }

        private object CallBestConstructor(ConstructorInfo[] constructors, ValueWithType[] parameters)
        {
            parameters ??= new ValueWithType[0];

            List<(ConstructorInfo constructor, ParameterInfo[] constructorParameters, int resolveCount, ValueWithType[] parameterValues)> candidates = new List<(ConstructorInfo constructor, ParameterInfo[] constructorParameters, int resolveCount, ValueWithType[] parameterValues)>();

            foreach (var constructor in constructors)
            {
                var constructorParameters = constructor.GetParameters();
                int constructorParameterCount = constructorParameters.Length;

                if (constructorParameterCount < parameters.Length)
                    continue;

                var constructorParameterValues = new ValueWithType[constructorParameterCount];
                int resolvedParametersCount = 0;

                if (parameters.Length > 0)
                {
                    for (int i = 0; i < constructorParameters.Length; i++)
                    {
                        foreach (var parameter in parameters)
                        {
                            if (parameter.Type.IsAssignableTo(constructorParameters[i].ParameterType))
                            {
                                constructorParameterValues[i] = parameter;
                                resolvedParametersCount++;
                                break;
                            }
                        }
                    }

                    if (resolvedParametersCount < parameters.Length)
                        continue;
                }

                int resolveCount = resolvedParametersCount + constructorParameters.Where((t, i) => constructorParameterValues[i] == null).Count(t => CanResolve(t.ParameterType));

                var candidate = (constructor, constructorParameters, resolveCount, constructorParameterValues);

                if (resolveCount == constructorParameterCount)
                {
                    // we have a perfect candidate
                    candidates.Clear();
                    candidates.Add(candidate);
                    break; 
                }
                else
                {
                    candidates.Add(candidate);
                }
            }

            if (candidates.Count == 0)
                return null;

            var bestCandidate = candidates[0];

            if (candidates.Count > 1)
                bestCandidate = candidates.OrderBy(c => c.constructorParameters.Length - c.resolveCount).ThenByDescending(c => c.constructorParameters.Length).First();

            for (int i= 0; i < bestCandidate.constructorParameters.Length; i++)
            {
                bestCandidate.parameterValues[i] ??= new ValueWithType(_Get(bestCandidate.constructorParameters[i].ParameterType));
            }

            return bestCandidate.constructor.Invoke(bestCandidate.parameterValues.Select(p => p.Value).ToArray());
        }

        private object _Get(Type type, ValueWithType[] parameters = null)
        {
            foreach (var service in Services.Where(svc => svc.IsMatch(type)))
            {
                object obj = service.Object;

                if (obj != null)
                    return obj;

                obj = CallBestConstructor(service.MatchingConstructors(type), parameters);

                if (obj != null)
                {
                    SetInjectProperties(obj);

                    if (service.Singleton)
                        service.Object = obj;

                    return obj;
                }
            }

            return null;
        }

        public object Get(Type type, params object[] parameters)
        {
            return _Get(type, parameters.Select(p => new ValueWithType(p)).ToArray());
        }

        private object _Create(Type type, ValueWithType[] parameters = null)
        {
            var obj = CallBestConstructor(new ServiceDefinition(type).Constructors, parameters);

            if (obj == null)
                return null;

            SetInjectProperties(obj);

            return obj;
        }

        public object Create(Type type)
        {
            return _Create(type);
        }

        public object Create(Type type, params object[] parameters)
        {
            return _Create(type, parameters.Select(p => new ValueWithType(p)).ToArray());
        }

        public T Create<T>() where T : class
        {
            return (T)_Create(typeof(T));
        }

        public T Create<T>(params object[] parameters) where T : class
        {
            return (T) _Create(typeof(T), parameters.Select(p => new ValueWithType(p)).ToArray());
        }

        public T Create<T,TParam1>(TParam1 param) where T : class
        {
            return (T)_Create(typeof(T), new ValueWithType[] {new ValueWithType<TParam1>(param)});
        }

        public T Create<T, TParam1, TParam2>(TParam1 param1, TParam2 param2) where T : class
        {
            return (T)_Create(typeof(T), new ValueWithType[] { new ValueWithType<TParam1>(param1), new ValueWithType<TParam2>(param2) });
        }

        public void UpdateDependencies(object o)
        {
            SetInjectProperties(o);
        }

        private void SetInjectProperties(object o)
        {
            if (o == null)
                return;

            var type = o.GetType();

            if (!_injectProperties.TryGetValue(type, out var injectProperties))
            {
                injectProperties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanWrite && p.GetCustomAttribute<InjectAttribute>() != null).ToArray();

                _injectProperties[type] = injectProperties;
            }

            foreach (var property in injectProperties)
            {
                if (CanResolve(property.PropertyType))
                    property.SetValue(o, Get(property.PropertyType));
            }
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

            return new ServiceRegistrationResult(this, serviceDefinition);
        }

        public IServiceRegistrationResult Register(Type type)
        {
            var serviceDefinition = new ServiceDefinition(type);

            _services.Add(serviceDefinition);

            return new ServiceRegistrationResult(this, serviceDefinition);
        }

        public IServiceRegistrationResult Register(Type type, object obj)
        {
            var serviceDefinition = new ServiceDefinition(type, obj);

            _services.Add(serviceDefinition);

            return new ServiceRegistrationResult(this, serviceDefinition);
        }

        public IServiceRegistrationResult Register<T>(T service)
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));

            var serviceDefinition = new ServiceDefinition(typeof(T), service);

            _services.Add(serviceDefinition);

            return new ServiceRegistrationResult(this, serviceDefinition);
        }

        public Type[] RegisteredTypes()
        {
            return _services.Select(svc => svc.RegistrationType).ToArray();
        }

        internal void RemoveConflicting(Type registrationType, ServiceDefinition svcDef)
        {
            _services.RemoveAll(svc => !ReferenceEquals(svcDef, svc) && svc.IsMatch(registrationType));
        }
    }
}