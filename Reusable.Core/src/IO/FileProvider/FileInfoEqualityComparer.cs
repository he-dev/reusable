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

        public bool Equals(IFileInfo x, IFileInfo y) => Equals(x?.Path, y?.Path);

        public int GetHashCode(IFileInfo obj) => GetHashCode(obj.Path);

        public bool Equals(string x, string y) => PathComparer.Equals(x, y);

        public int GetHashCode(string obj) => PathComparer.GetHashCode(obj);
    }
}