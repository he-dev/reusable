using System;

namespace Reusable.Shelly
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ParameterAttribute : Attribute
    {
        private const int CommandNamePosition = 0;

        private int _position = -1;

        public bool Mandatory { get; set; }

        public char ListSeparator { get; set; } = char.MinValue;

        public int Position
        {
            get { return _position; }
            set
            {
                if (!(value > CommandNamePosition))
                {
                    throw new ArgumentOutOfRangeException(nameof(Position), $"Position must be greater then {CommandNamePosition}.");
                }
                _position = value;
            }
        }
    }
}