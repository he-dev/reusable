using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;

namespace Reusable.IO
{
    internal class InMemoryFileInfo : IFileInfo
    {
        [CanBeNull]
        private readonly byte[] _data;

        [CanBeNull]
        private readonly IEnumerable<IFileInfo> _files;

        private InMemoryFileInfo([NotNull] string path)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
            ModifiedOn = DateTime.UtcNow;
        }

        public InMemoryFileInfo([NotNull] string path, byte[] data)
            : this(path)
        {
            _data = data;
            Exists = !(data is null);
            IsDirectory = false;
        }

        public InMemoryFileInfo([NotNull] string path, [NotNull] IEnumerable<IFileInfo> files)
            : this(path)
        {
            _files = files ?? throw new ArgumentNullException(nameof(files));
            Exists = true;
            IsDirectory = true;
        }

        #region IFileInfo

        public bool Exists { get; }

        public long Length => IsDirectory ? throw new InvalidOperationException("Directories have no length.") : _data?.Length ?? -1;

        public string Path { get; }

        public string Name => System.IO.Path.GetFileNameWithoutExtension(Path);

        public DateTime ModifiedOn { get; }

        public bool IsDirectory { get; }

        public Stream CreateReadStream()
        {
            return
                IsDirectory
                    ? throw new InvalidOperationException("Cannot create read-stream for a directory.")
                    : Exists
                        // ReSharper disable once AssignNullToNotNullAttribute - this is never null because it's protected by Exists.
                        ? new MemoryStream(_data)
                        : throw new InvalidOperationException("Cannot create a read-stream for a file that does not exist.");
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
