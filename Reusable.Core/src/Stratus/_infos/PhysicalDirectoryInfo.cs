using System;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Reusable.Stratus
{
    [PublicAPI]
    internal class PhysicalDirectoryInfo : ValueInfo
    {
        public PhysicalDirectoryInfo([NotNull] string name) : base(name) { }

        public override string Name { get; }

        public override bool Exists => Directory.Exists(Name);

        public override long? Length { get; }

        public override DateTime? CreatedOn { get; }

        public override DateTime? ModifiedOn => Exists ? Directory.GetLastWriteTimeUtc(Name) : default;

        public override Task CopyToAsync(Stream stream) => throw new NotSupportedException();

        public override Task<object> DeserializeAsync(Type targetType) => throw new NotSupportedException();
    }
}