using System;
using Reusable.Extensions;

namespace Reusable.ConfigWhiz.Data.Annotations
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SettingNamespaceAttribute : Attribute
    {
        private readonly string _name;
        public SettingNamespaceAttribute(string name) => _name = name.NullIfEmpty() ?? throw new ArgumentNullException(paramName: nameof(name), message: "Custom setting namespace must not be empty.");
        public override string ToString() => _name;
    }
}