using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Reusable.IO
{
    public class FileInfoEqualityComparer : IEqualityComparer<IFileInfo>, IEqualityComparer<string>
    {
        private static readonly IEqualityComparer PathComparer = StringComparer.OrdinalIgnoreCase;

        [NotNull]
        public static FileInfoEqualityComparer Default { get; } = new FileInfoEqualityComparer();

        public bool Equals(IFileInfo x, IFileInfo y)
        {
            return Equals(x?.Path, y?.Path);
        }

        public int GetHashCode(IFileInfo obj)
        {
            return GetHashCode(obj.Path);
        }

        public bool Equals(string x, string y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;

            return PathComparer.Equals(x, y);
        }

        public int GetHashCode(string obj)
        {
            return PathComparer.GetHashCode(obj);
        }
    }
}