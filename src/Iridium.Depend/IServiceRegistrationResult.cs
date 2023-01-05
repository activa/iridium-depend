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

namespace Iridium.Depend
{
    public interface IServiceRegistrationResult<out T,TReg> : IServiceRegistrationResult
    {
        new IServiceRegistrationResult<T, TNewReg> As<TNewReg>();
        new IServiceRegistrationResult<T,TReg> Scoped();
        new IServiceRegistrationResult<T,TReg> Singleton();
        new IServiceRegistrationResult<T, TReg> SkipDispose();
        IServiceRegistrationResult<T, TReg> OnCreate(Action<T, IServiceProvider> action);
        IServiceRegistrationResult<T, TReg> OnResolve(Action<T, IServiceProvider> action);
    }

    public interface IServiceRegistrationResult
    {
        IServiceRegistrationResult<T, T> As<T>();
        IServiceRegistrationResult AsSelf();
        IServiceRegistrationResult As(Type type);
        IServiceRegistrationResult Singleton();
        IServiceRegistrationResult Scoped();
        IServiceRegistrationResult SkipDispose();
        IServiceRegistrationResult IfNotRegistered();
        IServiceRegistrationResult IfNotRegistered<T>();
        IServiceRegistrationResult IfNotRegistered(Type type);
        IServiceRegistrationResult Replace();
        IServiceRegistrationResult Replace<T>();
        IServiceRegistrationResult Replace(Type type);
    }
}