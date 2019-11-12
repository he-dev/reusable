using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Reusable.Extensions;

namespace Reusable.Utilities.JsonNet.Annotations
{
    [UsedImplicitly]
    [AttributeUsage(AttributeTargets.Class)]
    public class NamespaceAttribute : Attribute, IEnumerable<string>
    {
        private readonly string _name;

        public NamespaceAttribute(string name) => _name = name;

        public string? Alias { get; set; }

        public override string ToString() => _name;

        public IEnumerator<string> GetEnumerator()
        {
            yield return _name;
            if (Alias.IsNotNullOrEmpty()) yield return Alias!;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}