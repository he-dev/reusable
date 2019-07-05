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
        UriString Uri { get; }
        
        bool Exists { get; }
        
        long Length { get; }
        
        DateTime CreateOn { get; }
        
        DateTime ModifiedOn { get; }
        
        MimeType Format { get; }

        Type DataType { get; }

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

        #region Resource

        public static IImmutableSession SetUri(this IImmutableSession session, UriString value) => session.SetItem(Resource.PropertySelector.Select(x => x.Uri), value);
        public static IImmutableSession SetExists(this IImmutableSession session, bool value) => session.SetItem(Resource.PropertySelector.Select(x => x.Exists), value);
        public static IImmutableSession SetFormat(this IImmutableSession session, MimeType value) => session.SetItem(Resource.PropertySelector.Select(x => x.Format), value);
        public static IImmutableSession SetDataType(this IImmutableSession session, Type value) => session.SetItem(Resource.PropertySelector.Select(x => x.DataType), value);

        public static UriString GetUri(this IImmutableSession session) => session.GetItemOrDefault(Resource.PropertySelector.Select(x => x.Uri));
        public static bool GetExists(this IImmutableSession session) => session.GetItemOrDefault(Resource.PropertySelector.Select(x => x.Exists));
        public static MimeType GetFormat(this IImmutableSession session) => session.GetItemOrDefault(Resource.PropertySelector.Select(x => x.Format), MimeType.None);
        public static Type GetDataType(this IImmutableSession session) => session.GetItemOrDefault(Resource.PropertySelector.Select(x => x.DataType));
        
        #endregion

        // Copies existing items from the specified session by T.
        public static IImmutableSession Copy<T>(this IImmutableSession session, From<T> from)
        {
            var selectors =
                from p in typeof(T).GetProperties()
                select Selector.FromProperty(typeof(T), p);

            var copyable =
                from selector in selectors
                where session.ContainsKey(selector.ToString())
                select selector;

            return copyable.Aggregate(ImmutableSession.Empty, (current, next) => current.SetItem(next.ToString(), session[next.ToString()]));
        }
    }
}