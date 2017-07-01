using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Reusable.CommandLine.Annotations
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CommandNamesAttribute : Attribute, IEnumerable<string>
    {
        private readonly IEnumerable<string> _names;

        public CommandNamesAttribute([NotNull] params string[] names)
        {
            _names = names ?? throw new ArgumentNullException(nameof(names));
        }

        public bool AllowShortName { get; set; }

        public string Namespace { get; set; }

        public bool NamespaceRequired { get; set; }

        public IEnumerator<string> GetEnumerator() => _names.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }


}