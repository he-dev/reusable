using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;

namespace Reusable.IO
{
    [PublicAPI]
    internal class PhysicalFileInfo : IFileInfo
    {
        public PhysicalFileInfo([NotNull] string path) => Path = path ?? throw new ArgumentNullException(nameof(path));

        #region IFileInfo

        public string Path { get; }

        public string Name => System.IO.Path.GetFileName(Path);

        public bool Exists => File.Exists(Path) || Directory.Exists(Path);

        public long Length => Exists && !IsDirectory ? new FileInfo(Path).Length : -1;

        public DateTime ModifiedOn => !string.IsNullOrWhiteSpace(Path) ? File.GetLastWriteTime(Path) : default;

        public bool IsDirectory => Directory.Exists(Path);

        public Stream CreateReadStream()
        {
            return
                IsDirectory
                    ? throw new InvalidOperationException($"Cannot open '{Path}' for reading because it's a directory.")
                    : Exists
                        ? File.OpenRead(Path)
                        : throw new InvalidOperationException("Cannot open '{Path}' for reading because the file does not exist.");
        }

        #endregion

        #region IEquatable<IFileInfo>

        public override bool Equals(object obj) => obj is IFileInfo file && Equals(file);

        public bool Equals(IFileInfo other) => FileInfoEqualityComparer.Default.Equals(other, this);

        public bool Equals(string other) => FileInfoEqualityComparer.Default.Equals(other, Path);

        public override int GetHashCode() => FileInfoEqualityComparer.Default.GetHashCode(this);

        #endregion
    }
}
