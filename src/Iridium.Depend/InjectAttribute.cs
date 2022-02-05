using System;
using System.Collections.Generic;
using System.Linq;

namespace Iridium.Depend
{
    [AttributeUsage(AttributeTargets.Property)]
    public class InjectAttribute : Attribute
    {
        public bool Required { get; set; }
    }
}