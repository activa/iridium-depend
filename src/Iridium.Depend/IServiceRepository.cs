using System;
using System.Collections.Generic;
using System.Linq;

namespace Iridium.Depend
{
    public interface IServiceRepository
    {
    // void UnRegister<T>();
    // void UnRegister(Type type);
    IServiceRegistrationResult<T,T> Register<T>();
    IServiceRegistrationResult Register(Type type);
    IServiceRegistrationResult Register(Type type, object obj);
    IServiceRegistrationResult<T,T> Register<T>(T service);
    IServiceRegistrationResult<T,T> Register<T>(Func<T> factoryMethod);
    IServiceRegistrationResult<T,T> Register<T>(Func<IServiceProvider, T> factoryMethod);
    IServiceRegistrationResult Register(Type registrationType, Func<object> factoryMethod);
    IServiceRegistrationResult Register(Type registrationType, Func<Type, object> factoryMethod);
    IServiceRegistrationResult Register(Type type, Func<IServiceProvider, Type, object> factoryMethod);
    Type[] ResolvableTypes();
    }
}