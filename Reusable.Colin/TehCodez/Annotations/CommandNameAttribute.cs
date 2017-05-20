using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Reusable.Colin.Annotations
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class CommandNameAttribute : Attribute, IEnumerable<string>
    {
        private readonly IEnumerable<string> _names;

        public CommandNameAttribute([NotNull] params string[] names)
        {
            if (names == null) throw new ArgumentNullException(nameof(names));
            if (names.Length == 0) throw new ArgumentException("You need to specify a least one command name.", nameof(names));

            _names = names;
        }

        public IEnumerator<string> GetEnumerator() => _names.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}