using System;
using System.Collections.Generic;
using System.Linq;

namespace Iridium.Depend
{
    public interface IServiceRepository
    {
        IServiceRepository CreateChild();
        T Get<T>();
        T Get<T>(params object[] parameters);
        T Get<T, TParam1>(TParam1 param);
        T Get<T, TParam1, TParam2>(TParam1 param1, TParam2 param2);
        object Get(Type type, params object[] parameters);
        object Create(Type type);
        object Create(Type type, params object[] parameters);
        T Create<T>() where T : class;
        T Create<T>(params object[] parameters) where T : class;
        T Create<T,TParam1>(TParam1 param) where T : class;
        T Create<T, TParam1, TParam2>(TParam1 param1, TParam2 param2) where T : class;
        void UpdateDependencies(object o);
        void UnRegister<T>();
        void UnRegister(Type type);
        IServiceRegistrationResult Register<T>();
        IServiceRegistrationResult Register(Type type);
        IServiceRegistrationResult Register(Type type, object obj);
        IServiceRegistrationResult Register<T>(T service);
    }
}