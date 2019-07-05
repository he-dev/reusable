using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Reusable.IOnymous
{
    public class ResourceEqualityComparer : IEqualityComparer<IResource>, IEqualityComparer<UriString>, IEqualityComparer<string>
    {
        private static readonly IEqualityComparer ResourceUriComparer = StringComparer.OrdinalIgnoreCase;

        [NotNull]
        public static ResourceEqualityComparer Default { get; } = new ResourceEqualityComparer();

        public bool Equals(IResource x, IResource y) => Equals(x?.Uri, y?.Uri);

        public int GetHashCode(IResource obj) => GetHashCode(obj.Uri);

        public bool Equals(UriString x, UriString y) => ResourceUriComparer.Equals(x, y);

        public int GetHashCode(UriString obj) => ResourceUriComparer.GetHashCode(obj);

        public bool Equals(string x, string y) => !string.IsNullOrWhiteSpace(x) && !string.IsNullOrWhiteSpace(y) && Equals(new UriString(x), new UriString(y));

        public int GetHashCode(string obj) => GetHashCode(new UriString(obj));
    }
}