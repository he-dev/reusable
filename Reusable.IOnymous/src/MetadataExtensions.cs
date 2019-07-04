using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Extensions;
using Reusable.Quickey;

namespace Reusable.IOnymous
{
    [UseType, UseMember]
    [TrimStart("I"), TrimEnd("Meta")]
    [PlainSelectorFormatter]
    public interface IRequestMeta : INamespace
    {
        Encoding Encoding { get; }

        //ISet<SoftString> Schemes { get; }

        CancellationToken CancellationToken { get; }
    }

    [UseType, UseMember]
    [TrimStart("I"), TrimEnd("Meta")]
    [PlainSelectorFormatter]
    public interface IProviderMeta : INamespace
    {
        [NotNull, ItemNotNull]
        IImmutableSet<SoftString> Schemes { get; }

        [NotNull, ItemNotNull]
        IImmutableSet<SoftString> Names { get; }

        bool AllowRelativeUri { get; }
    }

    [UseType, UseMember]
    [TrimStart("I"), TrimEnd("Meta")]
    [PlainSelectorFormatter]
    public interface IResourceMeta : INamespace
    {
        //SoftString Scheme { get; }

        MimeType Format { get; }

        Type Type { get; }

        string ActualName { get; }
    }

    public static class ImmutableSessionExtensions
    {
        private static readonly Selector<IImmutableSet<SoftString>> Schemes = From<IProviderMeta>.Select(x => x.Schemes);
        private static readonly Selector<IImmutableSet<SoftString>> Names = From<IProviderMeta>.Select(x => x.Names);

        public static IImmutableSession SetScheme(this IImmutableSession session, SoftString scheme)
        {
            if (scheme.IsNullOrEmpty())
            {
                return session;
            }
            
            return
                session.SetItem(Schemes, session.TryGetItem(Schemes, out var schemes)
                    ? schemes.Add(scheme)
                    : ImmutableHashSet<SoftString>.Empty.Add(scheme));
        }

        public static IImmutableSet<SoftString> GetSchemes(this IImmutableSession session)
        {
            return session.GetItemOrDefault(Schemes, ImmutableHashSet<SoftString>.Empty);
        }

        public static IImmutableSession SetName(this IImmutableSession session, SoftString name)
        {
            if (name.IsNullOrEmpty())
            {
                return session;
            }
            
            return
                session.SetItem(
                    Names,
                    session.TryGetItem(Names, out var names)
                        ? names.Add(name)
                        : ImmutableHashSet<SoftString>.Empty.Add(name));
        }

        public static IImmutableSet<SoftString> GetNames(this IImmutableSession session)
        {
            return session.GetItemOrDefault(Names, ImmutableHashSet<SoftString>.Empty);
        }
    }
}