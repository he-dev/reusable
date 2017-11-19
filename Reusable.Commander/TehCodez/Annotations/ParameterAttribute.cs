using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Reusable.CommandLine;

// ReSharper disable once CheckNamespace
namespace Reusable.Commander
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