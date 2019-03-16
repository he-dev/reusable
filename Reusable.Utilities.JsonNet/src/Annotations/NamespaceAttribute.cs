using System;
using JetBrains.Annotations;

namespace Reusable.Utilities.JsonNet.Annotations
{
    [UsedImplicitly]
    [AttributeUsage(AttributeTargets.Class)]
    public class NamespaceAttribute : Attribute
    {
        private readonly string _name;

        public NamespaceAttribute([NotNull] string name)
        {
            _name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public override string ToString() => _name;
    }
}