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
    internal class ServiceRegistrationResult<T,TReg> : ServiceRegistrationResult, IServiceRegistrationResult<T,TReg>
    {
        IServiceRegistrationResult<T, TNewReg> IServiceRegistrationResult<T, TReg>.As<TNewReg>() => As<TNewReg>();
        IServiceRegistrationResult<T, TReg> IServiceRegistrationResult<T, TReg>.Scoped() => new ServiceRegistrationResult<T, TReg>(Scoped());
        IServiceRegistrationResult<T, TReg> IServiceRegistrationResult<T, TReg>.Singleton() => new ServiceRegistrationResult<T, TReg>(Singleton());
        IServiceRegistrationResult<T, TReg> IServiceRegistrationResult<T, TReg>.SkipDispose() => new ServiceRegistrationResult<T, TReg>(SkipDispose());

        public ServiceRegistrationResult(ServiceRegistrationResult r) : base(r)
        {
        }

        public ServiceRegistrationResult(ServiceRepository repo, ServiceDefinition serviceDefinition) : base(repo, serviceDefinition)
        {
        }

        public new ServiceRegistrationResult<T, TNewReg> As<TNewReg>()
        {
            return new ServiceRegistrationResult<T, TNewReg>(base.As<TNewReg>());
        }

        public IServiceRegistrationResult<T, TReg> OnCreate(Action<T, IServiceProvider> action)
        {
            ServiceDefinition.AfterCreateActions.Add((obj, provider) => action((T)obj, provider));

            return this;
        }

        public IServiceRegistrationResult<T, TReg> OnResolve(Action<T, IServiceProvider> action)
        {
            ServiceDefinition.AfterResolveActions.Add((obj, provider) => action((T)obj, provider));

            return this;
        }
    }

    internal class ServiceRegistrationResult : IServiceRegistrationResult
    {
        protected readonly ServiceDefinition ServiceDefinition;
        protected readonly ServiceRepository Repository;

        IServiceRegistrationResult<T, T> IServiceRegistrationResult.As<T>() => As<T>();
        IServiceRegistrationResult IServiceRegistrationResult.As(Type type) => As(type);
        IServiceRegistrationResult IServiceRegistrationResult.Singleton() => Singleton();
        IServiceRegistrationResult IServiceRegistrationResult.Scoped() => Scoped();
        IServiceRegistrationResult IServiceRegistrationResult.SkipDispose() => SkipDispose();
        IServiceRegistrationResult IServiceRegistrationResult.IfNotRegistered<T>() => IfNotRegistered<T>();
        IServiceRegistrationResult IServiceRegistrationResult.IfNotRegistered(Type type) => IfNotRegistered(type);

        public IServiceRegistrationResult AsSelf()
        {
            ServiceDefinition.AddRegistrationType(ServiceDefinition.Type);

            Repository.TriggerChangeEvent();

            return this;
        }

        public ServiceRegistrationResult(ServiceRepository repo, ServiceDefinition serviceDefinition)
        {
            Repository = repo;
            ServiceDefinition = serviceDefinition;
        }

        public ServiceRegistrationResult(ServiceRegistrationResult r)
        {
            Repository = r.Repository;
            ServiceDefinition = r.ServiceDefinition;
        }

        public ServiceRegistrationResult<T,T> As<T>()
        {
            return new ServiceRegistrationResult<T,T>(As(typeof(T)));
        }

        public ServiceRegistrationResult As(Type type)
        {
            ServiceDefinition.AddRegistrationType(type);

            Repository.TriggerChangeEvent();

            return this;
        }

        public ServiceRegistrationResult Singleton()
        {
            ServiceDefinition.Lifetime = ServiceLifetime.Singleton;

            return this;
        }

        public ServiceRegistrationResult Scoped()
        {
            ServiceDefinition.Lifetime = ServiceLifetime.Scoped;

            return this;
        }

        public ServiceRegistrationResult SkipDispose()
        {
            ServiceDefinition.SkipDispose = true;

            return this;
        }

        public ServiceRegistrationResult IfNotRegistered<T>()
        {
            return IfNotRegistered(typeof(T));
        }

        public ServiceRegistrationResult IfNotRegistered(Type type)
        {
            if (Repository.ServiceDefinitions.SelectMany(s => s.RegistrationTypes).Any(t => t == type))
            {
                Repository.RemoveService(ServiceDefinition);
            }

            return this;
        }
    }
}