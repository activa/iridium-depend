using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Iridium.Depend.Extensions
{
    public class IridiumServiceProviderFactory : IServiceProviderFactory<ServiceRepository>
    {
        private ServiceRepository _repo;

        public IridiumServiceProviderFactory()
        {
        }

        public IridiumServiceProviderFactory(ServiceRepository repo)
        {
            _repo = repo;
        }

        public ServiceRepository CreateBuilder(IServiceCollection services)
        {
            _repo ??= new ServiceRepository();

            var repo = _repo;

            foreach (var item in services)
            {
                IServiceRegistrationResult svcReg = null;

                if (item.ImplementationType != null)
                    svcReg = repo.Register(item.ImplementationType).As(item.ServiceType);
                else if (item.ImplementationInstance != null)
                    svcReg = repo.Register(item.ImplementationInstance).As(item.ServiceType);
                else if (item.ImplementationFactory != null)
                    svcReg = repo.Register((provider) => item.ImplementationFactory(provider.Get<System.IServiceProvider>())).As(item.ServiceType);
                
                if (svcReg == null)
                    continue;

                switch (item.Lifetime)
                {
                    case Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton:
                        svcReg.Singleton();
                        break;
                    case Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped:
                        svcReg.Scoped();
                        break;
                    case Microsoft.Extensions.DependencyInjection.ServiceLifetime.Transient:
                        break;
                }
            }

            repo.Register<IridiumServiceProvider>();
            repo.Register<IridiumServiceScopeFactory>().Singleton();

            repo.Register(provider => provider);

            return repo;
        }

        public System.IServiceProvider CreateServiceProvider(ServiceRepository repo)
        {
            var serviceProvider = repo.CreateServiceProvider();

            return new IridiumServiceProvider(serviceProvider);
        }
    }
}