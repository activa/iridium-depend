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