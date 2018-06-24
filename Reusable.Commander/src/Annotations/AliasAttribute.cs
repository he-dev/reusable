using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Reusable.Commander.Annotations
{
    [UsedImplicitly]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class AliasAttribute : Attribute, IEnumerable<SoftString>
    {
        private readonly IEnumerable<SoftString> _values;
        
        public AliasAttribute([NotNull] params string[] values)
        {
            _values = values.Select(SoftString.Create).ToList();
        }

        public IEnumerator<SoftString> GetEnumerator() => _values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}