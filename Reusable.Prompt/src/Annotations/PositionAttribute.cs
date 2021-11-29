using System;
using JetBrains.Annotations;

namespace Reusable.Commander.Annotations
{
    [UsedImplicitly]
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class PositionAttribute : Attribute
    {
        public PositionAttribute(int position)
        {
            if (position < 1) throw new ArgumentOutOfRangeException(paramName: nameof(position), message: $"Command argument position must be greater than zero.");
            
            Value = position;
        }

        public int Value { get; }

        public static implicit operator int(PositionAttribute attribute) => attribute.Value;
    }
}