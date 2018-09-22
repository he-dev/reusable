using System;
using JetBrains.Annotations;

namespace Reusable.Commander.Annotations
{
    [UsedImplicitly]
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class PositionAttribute : Attribute
    {
        private const int CommandNameIndex = 0;
        private const int FirstArgumentIndex = 1;
        
        private readonly int _position;
        
        public PositionAttribute(int position)
        {
            if (position < FirstArgumentIndex) throw new ArgumentOutOfRangeException($"{nameof(position)} must be > {CommandNameIndex}.");
            _position = position;
        }

        public int Value => _position;

        public static implicit operator int(PositionAttribute attribute) => attribute.Value;
    }
}