using System;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Reusable.IOnymous
{
    [PublicAPI]
    internal class PhysicalFileInfo : ResourceInfo
    {
        public PhysicalFileInfo([NotNull] UriString uri) : base(uri) { }

        public override bool Exists => File.Exists(Uri.Path);

        public override long? Length => new FileInfo(Uri.Path).Length;

        public override DateTime? CreatedOn => Exists ? File.GetCreationTimeUtc(Uri.Path) : default;

        public override DateTime? ModifiedOn => Exists ? File.GetLastWriteTimeUtc(Uri.Path) : default;

        public override async Task CopyToAsync(Stream stream)
        {
            if (Exists)
            {
                using (var fileStream = File.OpenRead(Uri.Path))
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
                using (var fileStream = File.OpenRead(Uri.Path))
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
