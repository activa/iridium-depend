using System;
using Microsoft.Extensions.DependencyInjection;

namespace Iridium.Depend.Extensions
{
    public class IridiumServiceScope : IServiceScope
    {
        private readonly Iridium.Depend.IServiceProvider _iridiumScope;
        private readonly System.IServiceProvider _provider;

        public IridiumServiceScope(Iridium.Depend.IServiceProvider iridiumScope)
        {
            _iridiumScope = iridiumScope;
            _provider = new IridiumServiceProvider(iridiumScope);
        }

        public void Dispose()
        {
            _iridiumScope.Dispose();
        }

        public System.IServiceProvider ServiceProvider => _provider;
    }

}
