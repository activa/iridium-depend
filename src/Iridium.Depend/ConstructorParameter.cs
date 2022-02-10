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
    internal class ConstructorParameter
    {
        private readonly Type _type;
        private readonly string _name;
        private readonly object _value;
        private readonly Func<ServiceProvider, object> _factory;

        public string Name => _name;
        public Type Type => _type;
        
        public ConstructorParameter(Func<ServiceProvider,object> factory)
        {
            _factory = factory;
        }

        public ConstructorParameter(string name, object value, Type type)
        {
            _name = name;
            _value = value;
            _type = type;
        }

        public ConstructorParameter(object value)
        {
            _value = value;
            _type = value?.GetType();
        }

        public object Value(ServiceProvider serviceProvider) => _factory != null ? _factory(serviceProvider) : _value;

        internal static IEnumerable<ConstructorParameter> GenerateConstructorParameters(object[] parameters)
        {
            foreach (var parameter in parameters)
            {
                if (parameter.IsAnonymousType())
                {
                    foreach (var property in parameter.GetType().GetProperties())
                    {
                        yield return new ConstructorParameter(property.Name, property.GetValue(parameter), property.PropertyType);
                    }
                }
                else
                {
                    yield return new ConstructorParameter(parameter);
                }
            }
        }
    }
}