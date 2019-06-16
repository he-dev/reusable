using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Reusable.Data.Annotations
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Property)]
    public class TagsAttribute : Attribute, IEnumerable<string>
    {
        private readonly string[] _names;

        public TagsAttribute(params string[] names) => _names = names;

        public IEnumerator<string> GetEnumerator() => _names.Cast<string>().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}