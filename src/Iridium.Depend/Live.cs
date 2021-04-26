using System;
using System.Collections.Generic;
using System.Linq;

namespace Iridium.Depend
{
    public class Live<T>
    {
        private readonly Func<T> _factory;

        public Live(Func<T> factory) => _factory = factory;

        public T Value() => _factory();
    }
}