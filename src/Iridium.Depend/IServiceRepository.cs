#region License
//=============================================================================
// Iridium-Depend - Portable .NET Productivity Library 
//
// Copyright (c) 2008-2022 Philippe Leybaert
//
// Permission is hereby granted, free of charge, to any person obtaining a copy 
// of this software and associated documentation files (the "Software"), to deal 
// in the Software without restriction, including without limitation the rights 
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
// copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in 
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// IN THE SOFTWARE.
//=============================================================================
#endregion

using System;
using System.Collections.Generic;
using System.Linq;

namespace Iridium.Depend
{
    public interface IServiceRepository
    {
        void UnRegister<T>();
        void UnRegister(Type type);
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