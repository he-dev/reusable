using System;
using System.Collections;
using System.Collections.Generic;

namespace Reusable.Shelly
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CommandNameAttribute : Attribute
    {
        private readonly string _names;
        public CommandNameAttribute(string names) => _names = names ?? throw new ArgumentNullException(nameof(names));
        public override string ToString() => _names;
        public static implicit operator string(CommandNameAttribute commandNameAttribute) => commandNameAttribute.ToString();
    }
}