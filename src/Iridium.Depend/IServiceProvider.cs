using System;

namespace Iridium.Depend
{
    public interface IServiceProvider : IDisposable
    {
        T Resolve<T>();
        T Resolve<T>(params object[] parameters);
        object Resolve(Type type, params object[] parameters);
        T Get<T>();
        T Get<T>(params object[] parameters);
        object Get(Type type, params object[] parameters);
        object Create(Type type);
        object Create(Type type, params object[] parameters);
        T Create<T>() where T : class;
        T Create<T>(params object[] parameters) where T : class;
        void UpdateDependencies(object o);
        bool CanResolve(Type type);
        IServiceProvider CreateScope();
    }
}