using System;
using System.Collections.Generic;
using System.Linq.Custom;
using JetBrains.Annotations;

namespace Reusable.Quickey
{
    [PublicAPI]
    public interface ISelectorFormatter
    {
        string Format(IEnumerable<SelectorToken> tokens);
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Property)]
    public abstract class SelectorFormatterAttribute : Attribute, ISelectorFormatter
    {
        public abstract string Format(IEnumerable<SelectorToken> tokens);
    }

    [UsedImplicitly]
    public class JoinSelectorTokensAttribute : SelectorFormatterAttribute
    {
        public string Separator { get; set; } = string.Empty;
        
        public override string Format(IEnumerable<SelectorToken> tokens)
        {
            return tokens.Join(Separator);
        }
    }
}