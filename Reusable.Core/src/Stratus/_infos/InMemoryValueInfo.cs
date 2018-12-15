using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Reusable.Stratus
{
    internal class InMemoryValueInfo : ValueInfo
    {
        [CanBeNull]
        private readonly byte[] _data;

        [CanBeNull]
        private readonly IEnumerable<IValueInfo> _files;

        private InMemoryValueInfo([NotNull] string name) : base(name)
        {
            ModifiedOn = DateTime.UtcNow;
        }

        public InMemoryValueInfo([NotNull] string name, byte[] data)
            : this(name)
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

        public override async Task<object> DeserializeAsync(Type targetType)
        {
            using (var memoryStream = new MemoryStream(_data))
            using (var streamReader = new StreamReader(memoryStream))
            {
                return await streamReader.ReadToEndAsync();
            }
        }
    }
}
