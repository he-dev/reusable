using System;
using System.Collections;
using System.Collections.Generic;

namespace Reusable.Shelly
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class AliasAttribute : Attribute, IEnumerable<string>
    {
        private readonly IEnumerable<string> _names;

        public AliasAttribute(params string[] names)
        {
            if (names == null) throw new ArgumentNullException(nameof(names));
            _names = names;
        }

        public IEnumerator<string> GetEnumerator() => _names.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}