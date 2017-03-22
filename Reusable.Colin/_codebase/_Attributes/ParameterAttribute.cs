using System;

namespace Reusable.Shelly
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ParameterAttribute : Attribute
    {
        private const int CommandNamePosition = 0;
        private string _name;
        private int _position = -1;

        public string Name
        {
            get => _name;
            set { if (string.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(Name)); _name = value; }
        }

        public bool Required { get; set; }

        public char ListSeparator { get; set; } = char.MinValue;

        public int Position
        {
            get => _position; set
            {
                if (!(value > CommandNamePosition)) throw new ArgumentOutOfRangeException(nameof(Position), $"Position must be greater then {CommandNamePosition}.");
                _position = value;
            }
        }
    }
}