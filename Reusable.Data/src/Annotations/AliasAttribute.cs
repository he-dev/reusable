using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Reusable.Marbles;

namespace Reusable.Data.Annotations
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Property)]
    public class AliasAttribute : Attribute, IEnumerable<string>
    {
        private readonly ISet<string> _names;

        public AliasAttribute(params string[] names) => _names = new HashSet<string>(names, SoftString.Comparer);

        public IEnumerator<string> GetEnumerator() => _names.Cast<string>().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}