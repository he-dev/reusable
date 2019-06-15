using System;
using System.Linq.Custom;
using JetBrains.Annotations;

namespace Reusable.Quickey
{
    [PublicAPI]
    public interface ISelectorFormatter
    {
        string Format(Selector selector);
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Property)]
    public abstract class SelectorFormatterAttribute : Attribute, ISelectorFormatter
    {
        public abstract string Format(Selector selector);
    }

    public class PlainSelectorFormatterAttribute : SelectorFormatterAttribute
    {
        public override string Format(Selector selector)
        {
            return selector.Keys.Join(string.Empty);
        }
    }
}