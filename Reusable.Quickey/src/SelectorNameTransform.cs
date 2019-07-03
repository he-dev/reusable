using System;
using System.Collections.Generic;
using System.Linq.Custom;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace Reusable.Quickey
{
    public interface ISelectorNameTransform
    {
        [NotNull]
        string Apply(string name);
    }

    [UsedImplicitly]
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class)]
    public abstract class SelectorNameTransformAttribute : Attribute, ISelectorNameTransform
    {
        public abstract string Apply(string name);
    }

    public class RemoveAttribute : SelectorNameTransformAttribute
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