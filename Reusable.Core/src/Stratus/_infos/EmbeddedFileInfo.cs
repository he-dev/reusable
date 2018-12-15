using System;
using System.IO;
using System.Threading.Tasks;

namespace Reusable.Stratus
{
    internal class EmbeddedFileInfo : ValueInfo
    {
        private readonly Func<Stream> _getManifestResourceStream;

        public EmbeddedFileInfo(string name, Func<Stream> getManifestResourceStream) : base(name)
        {
            _getManifestResourceStream = getManifestResourceStream;
        }

        public override bool Exists => !(_getManifestResourceStream is null);

        public override long? Length
        {
            get
            {
                using (var stream = _getManifestResourceStream?.Invoke())
                {
                    return stream?.Length;
                }
            }
        }

        public override DateTime? CreatedOn { get; }

        public override DateTime? ModifiedOn { get; }


        public override async Task CopyToAsync(Stream stream)
        {
            if (Exists)
            {
                using (var resourceStream = _getManifestResourceStream())
                {
                    await resourceStream.CopyToAsync(stream);
                }
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public override async Task<object> DeserializeAsync(Type targetType)
        {
            using (var resourceStream = _getManifestResourceStream())
            using (var streamReader = new StreamReader(resourceStream))
            {
                return await streamReader.ReadToEndAsync();
            }
        }
    }
}