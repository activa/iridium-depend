using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Iridium.Depend.Extensions
{
    public class IridiumServiceProvider : System.IServiceProvider, IServiceProviderIsService, ISupportRequiredService, IDisposable
    {
        private readonly Iridium.Depend.IServiceProvider _serviceProvider;

        public IridiumServiceProvider(Iridium.Depend.IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public object GetService(Type serviceType)
        {
            return _serviceProvider.Get(serviceType);
        }

        public object GetRequiredService(Type serviceType)
        {
            return _serviceProvider.Get(serviceType) ?? throw new NotSupportedException();
        }

        public void Dispose()
        {
            _serviceProvider.Dispose();
        }

        public bool IsService(Type serviceType)
        {
            return _serviceProvider.CanResolve(serviceType);
        }
    }
}