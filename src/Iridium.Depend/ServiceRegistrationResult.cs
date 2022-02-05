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
        IServiceRegistrationResult IServiceRegistrationResult.IfNotRegistered<T>() => IfNotRegistered<T>();
        IServiceRegistrationResult IServiceRegistrationResult.IfNotRegistered(Type type) => IfNotRegistered(type);

        public IServiceRegistrationResult WireProperties()
        {
            ServiceDefinition.WireProperties = true;
            return this;
        }

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