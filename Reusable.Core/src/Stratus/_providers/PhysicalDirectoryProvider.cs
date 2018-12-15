using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Reusable.Stratus
{
    using static ValueProviderMetadataKeyNames;

    [PublicAPI]
    public class PhysicalDirectoryProvider : ValueProvider
    {
        public PhysicalDirectoryProvider()
            : base(
                ValueProviderMetadata.Empty
                    .Add(CanDeserialize, true)
                    .Add(CanSerialize, true)
                    .Add(CanDelete, true)
            )
        { }

        public override Task<IValueInfo> GetValueInfoAsync(string name, ValueProviderMetadata metadata = null)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            return Task.FromResult<IValueInfo>(new PhysicalFileInfo(name));
        }

        public override async Task<IValueInfo> SerializeAsync(string name, Stream value, ValueProviderMetadata metadata = null)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            using (var streamReader = new StreamReader(value))
            {
                var fullName = Path.Combine(name, await streamReader.ReadToEndAsync());
                try
                {
                    Directory.CreateDirectory(fullName);
                    return await GetValueInfoAsync(fullName, metadata);
                }
                catch (Exception inner)
                {
                    throw CreateException(this, fullName, metadata, inner);
                }
            }
        }

        public override async Task<IValueInfo> SerializeAsync(string name, object value, ValueProviderMetadata metadata = null)
        {
            var binaryFormatter = new BinaryFormatter();
            using (var memoryStream = new MemoryStream())
            {
                binaryFormatter.Serialize(memoryStream, value);
                return await SerializeAsync(name, memoryStream, metadata);
            }
        }

        public override async Task<IValueInfo> DeleteAsync(string name, ValueProviderMetadata metadata = null)
        {
            try
            {
                Directory.Delete(name, true);
                return await GetValueInfoAsync(name, metadata);
            }
            catch (Exception inner)
            {
                throw CreateException(this, name, metadata, inner);
            }
        }
    }
}