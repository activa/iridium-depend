#region License
//=============================================================================
// Iridium-Depend - Portable .NET Productivity Library 
//
// Copyright (c) 2008-2022 Philippe Leybaert
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

namespace Iridium.Depend
{
    internal class ServiceDefinition
    {
        private List<Type> _registrationTypes;
        private List<Type> _defaultRegistrationTypes;
        private readonly ServiceConstructor[] _serviceConstructors;

        public IEnumerable<Type> RegistrationTypes => _registrationTypes ?? (_defaultRegistrationTypes ??= GenerateDefaultRegistrationTypes());

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
        public List<Action<object, IServiceProvider>> AfterCreateActions = new();
        public List<Action<object, IServiceProvider>> AfterResolveActions = new();
        public ConstructorCandidate BestConstructorCandidate { get; private set; }

        public ServiceConstructor[] ServiceConstructors => _serviceConstructors;

        public ServiceDefinition(Type type, object obj = null, ServiceLifetime lifetime = ServiceLifetime.Transient, Func<IServiceProvider, Type, object> factoryMethod = null)
        {
            RegisteredObject = obj;
            Type = type;
            Lifetime = lifetime;
            Factory = factoryMethod;

            if (obj != null)
                SkipDispose = true;

            IsOpenGenericType = type.IsGenericTypeDefinition;

            _serviceConstructors = (from c in Type.GetConstructors() select new ServiceConstructor(this, c)).ToArray();
        }

        private List<Type> GenerateDefaultRegistrationTypes()
        {
            var registrationTypes = Type.GetInterfaces().AsEnumerable();

            if (IsOpenGenericType)
            {
                registrationTypes = registrationTypes.Select(_ => _.IsGenericType ? _.GetGenericTypeDefinition() : _);
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

        public void PreResolve(ServiceResolver serviceResolver)
        {
            foreach (var serviceConstructor in _serviceConstructors)
            {
                serviceConstructor.PreResolve(serviceResolver);
            }

            BestConstructorCandidate = _serviceConstructors.Select(_ => _.ConstructorCandidate).OrderByDescending(_ => _.MatchScore).FirstOrDefault();
        }
    }
}