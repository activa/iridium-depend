#region License
//=============================================================================
// Iridium-Core - Portable .NET Productivity Library 
//
// Copyright (c) 2008-2017 Philippe Leybaert
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

namespace Iridium.Depend
{
    public static class ServiceLocator
    {
        public static T Get<T>() => ServiceRepository.Default.Get<T>();
        public static object Get(Type type) => ServiceRepository.Default.Get(type);

        public static object Create(Type type) => ServiceRepository.Default.Create(type);
        public static T Create<T>() where T:class => ServiceRepository.Default.Create<T>();
        //public static object Create(Type type, params (Type t, object value)[] parameters) => ServiceRepository.Default.Create(type, parameters);
        //public static T Create<T>(params object[] parameters) where T : class => ServiceRepository.Default.Create<T>(parameters);

        public static void UnRegister<T>() => ServiceRepository.Default.UnRegister<T>();
        public static void UnRegister(Type type) => ServiceRepository.Default.UnRegister(type);

        public static IServiceRegistrationResult Register<T>() => ServiceRepository.Default.Register<T>();
        public static IServiceRegistrationResult Register(Type type) => ServiceRepository.Default.Register(type);
        public static IServiceRegistrationResult Register<T>(T service) => ServiceRepository.Default.Register<T>(service);

        public static void UpdateDependencies(object obj) => ServiceRepository.Default.UpdateDependencies(obj);
    }
}