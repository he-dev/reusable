using System.Collections.Immutable;
using JetBrains.Annotations;
using Reusable.Quickey;

namespace Reusable.IOnymous
{
    [UseType, UseMember]
    [TrimStart("I"), TrimEnd("Properties")]
    [PlainSelectorFormatter]
    public interface IProviderProperties
    {
        [NotNull, ItemNotNull]
        IImmutableSet<SoftString> Schemes { get; }

        [NotNull, ItemNotNull]
        IImmutableSet<SoftString> Names { get; }

        bool AllowRelativeUri { get; }
    }
}