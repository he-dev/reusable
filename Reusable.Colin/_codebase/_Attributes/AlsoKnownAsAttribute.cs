using System;
using System.Collections;
using System.Collections.Generic;

namespace Reusable.Colin
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class AlsoKnownAsAttribute : Attribute, IEnumerable<string>
    {
        private readonly IEnumerable<string> _names;

        public AlsoKnownAsAttribute(params string[] names) => _names = names ?? throw new ArgumentNullException(nameof(names));

        public IEnumerator<string> GetEnumerator() => _names.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}