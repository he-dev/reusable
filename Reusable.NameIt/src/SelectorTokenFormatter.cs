using System;
using System.Collections.Generic;
using System.Linq.Custom;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace Reusable.Quickey
{
    public interface ISelectorTokenFormatter
    {
        Type? Token { get; set; }
        
        string Format(string name);
    }

    [UsedImplicitly]
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, Inherited = false)]
    public abstract class SelectorTokenFormatterAttribute : Attribute, ISelectorTokenFormatter
    {
        public Type? Token { get; set; }
        
        public abstract string Format(string name);
    }
    
    [UsedImplicitly]
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Property)]
    public class ReplaceAttribute : SelectorTokenFormatterAttribute
    {
        private readonly string _pattern;
        private readonly string _name;

        public ReplaceAttribute([RegexPattern] string pattern, string name)
        {
            _pattern = pattern;
            _name = name;
        }

        public override string Format(string name)
        {
            return Regex.Replace(name, _pattern, _name);
        }
    }

    public class TrimEndAttribute : ReplaceAttribute
    {
        public TrimEndAttribute(string suffix) : base($"{suffix}$", string.Empty) { }
    }

    public class TrimStartAttribute : ReplaceAttribute
    {
        public TrimStartAttribute(string prefix) : base($"^{prefix}", string.Empty) { }
    }
}