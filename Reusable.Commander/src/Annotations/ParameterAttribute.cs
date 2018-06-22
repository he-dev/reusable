using System;
using JetBrains.Annotations;

namespace Reusable.Commander.Annotations
{
    [UsedImplicitly]
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class ParameterAttribute : Attribute
    {
        private int _position;

        public int Position
        {
            get => _position;
            set
            {
                if(!(value > CommandLine.CommandIndex)) throw new ArgumentOutOfRangeException(
                    paramName: nameof(Position), 
                    message: $"{nameof(Position)} must be > {CommandLine.CommandIndex} ({nameof(CommandLine.CommandIndex)}).");
                _position = value;
            }
        }
    }
}