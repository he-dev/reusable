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

        public PositionAttribute(int position)
        {
            if (position < FirstArgumentIndex) throw new ArgumentOutOfRangeException($"{nameof(position)} must be > {CommandNameIndex}.");
            Value = position;
        }

        public int Value { get; }

        public static implicit operator int(PositionAttribute attribute) => attribute.Value;
    }

    [UsedImplicitly]
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class GroupAttribute : Attribute
    {
        public GroupAttribute(int index)
        {
            Index = index;
        }
        
        public int Index { get; }
    }
}