using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Reusable.IOnymous
{
    public class ResourceInfoEqualityComparer : IEqualityComparer<IResourceInfo>, IEqualityComparer<UriString>, IEqualityComparer<string>
    {
        private static readonly IEqualityComparer ResourceUriComparer = StringComparer.OrdinalIgnoreCase;

        [NotNull]
        public static ResourceInfoEqualityComparer Default { get; } = new ResourceInfoEqualityComparer();

        public bool Equals(IResourceInfo x, IResourceInfo y) => Equals(x?.Uri, y?.Uri);

        public int GetHashCode(IResourceInfo obj) => GetHashCode(obj.Uri);

        public bool Equals(UriString x, UriString y) => ResourceUriComparer.Equals(x, y);

        public int GetHashCode(UriString obj) => ResourceUriComparer.GetHashCode(obj);

        public bool Equals(string x, string y) => !string.IsNullOrWhiteSpace(x) && !string.IsNullOrWhiteSpace(y) && Equals(new UriString(x), new UriString(y));

        public int GetHashCode(string obj) => GetHashCode(new UriString(obj));
    }
}