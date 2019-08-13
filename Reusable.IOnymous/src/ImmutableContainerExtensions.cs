using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Reusable.Data;
using Reusable.Quickey;

namespace Reusable.IOnymous
{
    public static class ImmutableContainerExtensions
    {
        //private static readonly Selector<IImmutableSet<SoftString>> Schemes = From<IProviderProperties>.Select(x => x.Schemes);
        //private static readonly Selector<IImmutableSet<SoftString>> Names = From<IProviderProperties>.Select(x => x.Names);

        public static IImmutableContainer SetScheme(this IImmutableContainer container, SoftString scheme)
        {
            if (scheme.IsNullOrEmpty())
            {
                return container;
            }

            return
                container.SetItem(ResourceProviderProperty.Schemes, container.TryGetItem(ResourceProviderProperty.Schemes, out var schemes)
                    ? schemes.Add(scheme)
                    : ImmutableHashSet<SoftString>.Empty.Add(scheme));
        }

        public static IImmutableSet<SoftString> GetSchemes(this IImmutableContainer container)
        {
            return container.GetItemOrDefault(ResourceProviderProperty.Schemes, ImmutableHashSet<SoftString>.Empty);
        }

//        public static IImmutableContainer SetName(this IImmutableContainer container, SoftString name)
//        {
//            if (name.IsNullOrEmpty())
//            {
//                return container;
//            }
//
//
//            if (container.TryGetItem(ResourceProviderProperty.Tags, out var names))
//            {
//                names = names.Add(name);
//            }
//            else
//            {
//                names = ImmutableSortedSet.Create<SoftString>(new ResourceProviderNameComparer()).Add(name);
//            }
//
//            return container.SetItem(ResourceProviderProperty.Tags, names);
//        }
        
        public static IImmutableContainer AddTag(this IImmutableContainer container, SoftString tag)
        {
            if (tag.IsNullOrEmpty())
            {
                return container;
            }


            if (container.TryGetItem(ResourceProviderProperty.Tags, out var tags))
            {
                tags = tags.Add(tag);
            }
            else
            {
                tags = ImmutableSortedSet<SoftString>.Empty.Add(tag);
            }

            return container.SetItem(ResourceProviderProperty.Tags, tags);
        }

        public static IImmutableSet<SoftString> Tags(this IImmutableContainer container)
        {
            return container.GetItemOrDefault(ResourceProviderProperty.Tags, ImmutableHashSet<SoftString>.Empty);
        }

        #region Resource

        public static IImmutableContainer SetUri(this IImmutableContainer container, UriString value) => container.SetItem(ResourceProperty.Uri, value);
        public static IImmutableContainer SetExists(this IImmutableContainer container, bool value) => container.SetItem(ResourceProperty.Exists, value);
        public static IImmutableContainer SetFormat(this IImmutableContainer container, MimeType value) => container.SetItem(ResourceProperty.Format, value);

        public static IImmutableContainer SetDataType(this IImmutableContainer container, Type value) => container.SetItem(ResourceProperty.DataType, value);

        public static UriString GetUri(this IImmutableContainer container) => container.GetItemOrDefault(ResourceProperty.Uri);
        public static bool GetExists(this IImmutableContainer container) => container.GetItemOrDefault(ResourceProperty.Exists);
        public static MimeType GetFormat(this IImmutableContainer container) => container.GetItemOrDefault(ResourceProperty.Format, MimeType.None);
        public static Type GetDataType(this IImmutableContainer container) => container.GetItemOrDefault(ResourceProperty.DataType);

        #endregion

        // Copies existing items from the specified session by T.
        public static IImmutableContainer Copy(this IImmutableContainer container, IEnumerable<Selector> selectors)
        {
            var copyable =
                from selector in selectors
                where container.ContainsKey(selector.ToString())
                select selector;

            return copyable.Aggregate(ImmutableContainer.Empty, (current, next) => current.SetItem(next.ToString(), container[next.ToString()]));
        }

        public static IImmutableContainer CopyRequestProperties(this IImmutableContainer container)
        {
            return container.Copy(RequestProperty.Selectors);
        }

        public static IImmutableContainer CopyResourceProperties(this IImmutableContainer container)
        {
            return container.Copy(ResourceProperty.Selectors);
        }
    }
}