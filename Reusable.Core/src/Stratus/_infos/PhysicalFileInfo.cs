using System;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Reusable.Stratus
{
    [PublicAPI]
    internal class PhysicalFileInfo : ValueInfo
    {
        public PhysicalFileInfo([NotNull] string name) : base(name) { }

        public override bool Exists => File.Exists(Name);

        public override long? Length => new FileInfo(Name).Length;

        public override DateTime? CreatedOn { get; }

        public override DateTime? ModifiedOn => Exists ? File.GetLastWriteTimeUtc(Name) : default;

        public override async Task CopyToAsync(Stream stream)
        {
            if (Exists)
            {
                using (var fileStream = File.OpenRead(Name))
                {
                    await fileStream.CopyToAsync(stream);
                }
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public override async Task<object> DeserializeAsync(Type targetType)
        {
            if (Exists)
            {
                using (var fileStream = File.OpenRead(Name))
                using (var streamReader = new StreamReader(fileStream))
                {
                    return await streamReader.ReadToEndAsync();
                }
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
    }
}
