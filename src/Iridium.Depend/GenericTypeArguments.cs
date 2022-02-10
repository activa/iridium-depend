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
using System.Linq;

namespace Iridium.Depend
{
    public class TypeCollection
    {
        private readonly Type[] _types;

        public TypeCollection(Type[] types)
        {
            _types = types;
        }

        public override int GetHashCode()
        {
            var hashCode = 0;

            foreach (var type in _types)
            {
                hashCode ^= type.GetHashCode();
            }

            return hashCode;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is TypeCollection other))
                return false;

            if (other._types.Length != _types.Length)
                return false;

            for (int i = 0; i < _types.Length; i++)
                if (_types[i] != other._types[i])
                    return false;

            return true;
        }

        public override string ToString()
        {
            return string.Join(",", _types.Select(_ => _.Name));
        }
    }
}