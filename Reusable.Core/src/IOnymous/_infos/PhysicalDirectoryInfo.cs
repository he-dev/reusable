using System;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Reusable.IOnymous
{
    [PublicAPI]
    internal class PhysicalDirectoryInfo : ResourceInfo
    {
        public PhysicalDirectoryInfo([NotNull] SimpleUri uri) : base(uri) { }

        public override SimpleUri Uri { get; }

        public override bool Exists => Directory.Exists(Uri.Path);

        public override long? Length { get; }

        public override DateTime? CreatedOn { get; }

        public override DateTime? ModifiedOn => Exists ? Directory.GetLastWriteTimeUtc(Uri.Path) : default;

        public override Task CopyToAsync(Stream stream) => throw new NotSupportedException();

        public override Task<object> DeserializeAsync(Type targetType) => throw new NotSupportedException();
    }
}