using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Iridium.Depend.Extensions
{
    public class IridiumServiceScopeFactory : IServiceScopeFactory
    {
        private readonly Iridium.Depend.IServiceProvider _serviceProvider;

        public IridiumServiceScopeFactory(Iridium.Depend.IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IServiceScope CreateScope()
        {
            return new IridiumServiceScope(_serviceProvider.CreateScope());
        }
    }
}