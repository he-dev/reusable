using System;
using System.Collections.Generic;
using System.Linq.Custom;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace Reusable.Quickey
{
    public interface ISelectorTokenFilter
    {
        [NotNull]
        string Apply(string name);
    }

    [UsedImplicitly]
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, Inherited = false)]
    public abstract class SelectorTokenFilterAttribute : Attribute, ISelectorTokenFilter
    {
        public abstract string Apply(string name);
    }
    
    [UsedImplicitly]
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Property)]
    public class RenameAttribute : SelectorTokenFilterAttribute
    {
        private readonly string _name;

        public RenameAttribute(string name) => _name = name;

        public override string Apply(string name)
        {
            return _name;
        }
    }

    public class RemoveAttribute : SelectorTokenFilterAttribute
    {
        private readonly IEnumerable<string> _patterns;

        public RemoveAttribute([RegexPattern] params string[] patterns) => _patterns = patterns;

        public override string Apply(string name)
        {
            return Regex.Replace(name, $"({_patterns.Join("|")})", string.Empty);
        }
    }

    public class TrimEndAttribute : RemoveAttribute
    {
        public TrimEndAttribute([RegexPattern] string suffixPattern) : base($"{suffixPattern}$") { }
    }

    public class TrimStartAttribute : RemoveAttribute
    {
        public TrimStartAttribute([RegexPattern] string prefixPattern) : base($"^{prefixPattern}") { }
    }
}