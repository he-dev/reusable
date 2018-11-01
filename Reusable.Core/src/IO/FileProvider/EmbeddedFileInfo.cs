using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Reusable.IO
{
    internal class EmbeddedFileInfo : IFileInfo
    {
        private readonly Func<Stream> _getManifestResourceStream;

        public EmbeddedFileInfo(string path, Func<Stream> getManifestResourceStream)
        {
            _getManifestResourceStream = getManifestResourceStream;
            Path = path;
        }

        public string Path { get; }

        public string Name => System.IO.Path.GetFileNameWithoutExtension(Path);

        public bool Exists => !(_getManifestResourceStream is null);

        public long Length => _getManifestResourceStream()?.Length ?? -1;

        public DateTime ModifiedOn { get; }

        public bool IsDirectory => false;

        // No protection necessary because there are no embedded directories.
        public Stream CreateReadStream() => _getManifestResourceStream();

        //public IEnumerator<IFileInfo> GetEnumerator()
        //{
        //    throw new NotSupportedException($"{nameof(EmbeddedFileInfo)} does not support enumerator.");
        //}

        //IEnumerator IEnumerable.GetEnumerator()
        //{
        //    return GetEnumerator();
        //}

        #region IEquatable<IFileInfo>

        public override bool Equals(object obj) => obj is IFileInfo file && Equals(file);

        public bool Equals(IFileInfo other) => FileInfoEqualityComparer.Default.Equals(other, this);

        public bool Equals(string other) => FileInfoEqualityComparer.Default.Equals(other, Path);

        public override int GetHashCode() => FileInfoEqualityComparer.Default.GetHashCode(this);

        #endregion
    }
}