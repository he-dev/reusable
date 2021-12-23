using JetBrains.Annotations;
using Reusable.Essentials;

namespace Reusable.Fluorite.Html;

[PublicAPI]
public class CssRuleSet
{
    public SoftString Selector { get; set; }

    public string Declarations { get; set; }
}