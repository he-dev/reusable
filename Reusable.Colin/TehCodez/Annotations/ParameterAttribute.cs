using System;
using System.Linq;

namespace Reusable.Colin.Annotations
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ParameterAttribute : Attribute
    {
        private const int CommandNamePosition = 0;
        private int _position = -1;
        private bool _required;
        private bool _canCreateShortName = true;

        public ParameterAttribute(params string[] names)
        {
            Names = names;
        }

        public string[] Names { get; }

        /// <summary>
        /// Gets or sets a value indication whether this paramter is required. It is automatically true if the Position is greater then 0.
        /// </summary>
        public bool Required
        {
            get => Position > CommandNamePosition || _required;
            set => _required = value;
        }

        /// <summary>
        /// Indicates whether a short name should be created from the capital letters. It is false if Names is not empty.
        /// </summary>
        public bool CanCreateShortName
        {
            get => !Names.Any() && _canCreateShortName;
            set => _canCreateShortName = value;
        }

        /// <summary>
        /// Gets or sets the position of the parameter.
        /// </summary>
        public int Position
        {
            get => _position; set
            {
                if (!(value > CommandNamePosition)) throw new ArgumentOutOfRangeException(nameof(Position), $"Position must be greater then {CommandNamePosition}.");
                _position = value;
            }
        }

        public void Deconstruct(out bool required, out int position)
        {
            required = Required;
            position = Position;
        }
    }
}