using System;

namespace Iridium.Depend
{
    public partial class ServiceRepository
    {
        private IServiceProvider _compatServiceProvider = null;

        [Obsolete("Use IServiceProvider")]
        public T Get<T>()
        {
            _compatServiceProvider ??= CreateServiceProvider();

            return _compatServiceProvider.Get<T>();
        }

        [Obsolete("Use IServiceProvider")]
        public T Get<T>(params object[] parameters)
        {
            _compatServiceProvider ??= CreateServiceProvider();

            return _compatServiceProvider.Get<T>(parameters);
        }

        [Obsolete("Use IServiceProvider")]
        public object Get(Type type, params object[] parameters)
        {
            _compatServiceProvider ??= CreateServiceProvider();

            return _compatServiceProvider.Get(type, parameters);
        }

        [Obsolete("Use IServiceProvider")]
        public object Create(Type type)
        {
            _compatServiceProvider ??= CreateServiceProvider();

            return _compatServiceProvider.Create(type);
        }

        [Obsolete("Use IServiceProvider")]
        public object Create(Type type, params object[] parameters)
        {
            _compatServiceProvider ??= CreateServiceProvider();

            return _compatServiceProvider.Create(type, parameters);
        }

        [Obsolete("Use IServiceProvider")]
        public T Create<T>() where T : class
        {
            _compatServiceProvider ??= CreateServiceProvider();

            return _compatServiceProvider.Create<T>();
        }

        [Obsolete("Use IServiceProvider")]
        public T Create<T>(params object[] parameters) where T : class
        {
            _compatServiceProvider ??= CreateServiceProvider();

            return _compatServiceProvider.Create<T>(parameters);
        }

        [Obsolete("Use IServiceProvider")]
        public void UpdateDependencies(object o)
        {
            _compatServiceProvider ??= CreateServiceProvider();

            _compatServiceProvider.UpdateDependencies(o);
        }

        [Obsolete("Use IServiceProvider")]
        public bool CanResolve(Type type)
        {
            _compatServiceProvider ??= CreateServiceProvider();

            return _compatServiceProvider.CanResolve(type);
        }
    }
}