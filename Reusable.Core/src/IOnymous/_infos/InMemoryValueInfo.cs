using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Reusable.IOnymous
{
    internal class InMemoryResourceInfo : ResourceInfo
    {
        [CanBeNull]
        private readonly byte[] _data;

        [CanBeNull]
        private readonly IEnumerable<IResourceInfo> _files;

        public InMemoryResourceInfo([NotNull] SimpleUri uri) : this(uri, new byte[0])
        {
            ModifiedOn = DateTime.UtcNow;
        }

        public InMemoryResourceInfo([NotNull] SimpleUri uri, byte[] data)
            : base(uri)
        {
            _data = data;
            Exists = !(data is null);
        }

        public override bool Exists { get; }

        public override long? Length => _data?.Length;

        public override DateTime? CreatedOn { get; }

        public override DateTime? ModifiedOn { get; }

        public override async Task CopyToAsync(Stream stream)
        {
            if (Exists)
            {
                await stream.WriteAsync(_data, 0, _data.Length);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public override Task<object> DeserializeAsync(Type targetType)
        {
            if (!Exists)
            {
                throw new InvalidOperationException($"Cannot deserialize '{Uri}' because it doesn't exist.");
            }

            var binaryFormatter = new BinaryFormatter();
            
            using (var memoryStream = new MemoryStream(_data))
            //using (var streamReader = new StreamReader(memoryStream))
            {
                return Task.FromResult(binaryFormatter.Deserialize(memoryStream));
                //return await streamReader.ReadToEndAsync();
            }
        }
    }
}
