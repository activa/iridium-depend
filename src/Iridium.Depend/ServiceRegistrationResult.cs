using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Iridium.Depend
{
    internal class ServiceRegistrationResult : IServiceRegistrationResult
    {
        private readonly ServiceDefinition _svc;
        private readonly ServiceRepository _repository;

        public ServiceRegistrationResult(ServiceRepository repo, ServiceDefinition svc)
        {
            _repository = repo;
            _svc = svc;
        }

        public IServiceRegistrationResult As<T>()
        {
            return As(typeof(T));
        }

        public IServiceRegistrationResult As(Type type)
        {
            if (!_svc.IsUnboundGenericType && !_svc.Type.IsAssignableTo(type))
                throw new ArgumentException("Type is not compatible");

            _svc.RegistrationType = type;

            return this;
        }

        public IServiceRegistrationResult Singleton()
        {
            _svc.Singleton = true;

            return this;
        }

        public IServiceRegistrationResult Replace<T>()
        {
            _repository.RemoveConflicting(typeof(T), _svc);

            return this;
        }

        public IServiceRegistrationResult Replace(Type type)
        {
            _repository.RemoveConflicting(type, _svc);

            return this;
        }

        public Type RegisteredAsType => _svc.RegistrationType;
    }
}