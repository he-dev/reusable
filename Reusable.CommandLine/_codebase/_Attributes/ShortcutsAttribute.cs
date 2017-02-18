using System;
using System.Collections;
using System.Collections.Generic;

namespace Reusable.Shelly
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class ShortcutsAttribute : Attribute, IEnumerable<string>
    {
        private readonly IEnumerable<string> _names;

        public ShortcutsAttribute(params string[] names) => _names = names ?? throw new ArgumentNullException(nameof(names));

        public IEnumerator<string> GetEnumerator() => _names.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}