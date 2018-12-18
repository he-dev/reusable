using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Exceptionizer;
using Reusable.Extensions;
using Reusable.Flawless;

namespace Reusable.IOnymous
{
    using static ResourceMetadataKeys;

    [PublicAPI]
    public interface IResourceProvider
    {
        [NotNull]
        ResourceMetadata Metadata { get; }

        [ItemNotNull]
        Task<IResourceInfo> GetAsync([NotNull] UriString uri, ResourceMetadata metadata = null);

        [ItemNotNull]
        Task<IResourceInfo> PostAsync([NotNull] UriString uri, [NotNull] Stream value, ResourceMetadata metadata = null);

        [ItemNotNull]
        Task<IResourceInfo> PutAsync([NotNull] UriString uri, [NotNull] Stream value, ResourceMetadata metadata = null);

        [ItemNotNull]
        Task<IResourceInfo> DeleteAsync([NotNull] UriString uri, ResourceMetadata metadata = null);
    }

    public abstract class ResourceProvider : IResourceProvider
    {
        //protected static readonly IExpressValidator<UriString> SchemeValidator = ExpressValidator.For<UriString>(assert =>
        //{
        //    assert.True()
        //};

        public static readonly string Scheme = "ionymous";

        protected ResourceProvider(ResourceMetadata metadata)
        {
            // If this is a decorator then the decorated resource-provider already has set this.
            if (!metadata.ContainsKey(ProviderDefaultName))
            {
                metadata = metadata.Add(ProviderDefaultName, GetType().ToPrettyString());
            }

            Metadata = metadata;
        }

        public virtual ResourceMetadata Metadata { get; }

        public abstract Task<IResourceInfo> GetAsync(UriString uri, ResourceMetadata metadata = null);

        public virtual Task<IResourceInfo> PostAsync(UriString name, Stream value, ResourceMetadata metadata = null) { throw new NotImplementedException(); }

        public abstract Task<IResourceInfo> PutAsync(UriString uri, Stream value, ResourceMetadata metadata = null);

        public abstract Task<IResourceInfo> DeleteAsync(UriString uri, ResourceMetadata metadata = null);

        protected static Exception CreateException(IResourceProvider provider, string name, ResourceMetadata metadata, Exception inner, [CallerMemberName] string memberName = null)
        {
            return new Exception();
        }

        protected UriString ValidateScheme([NotNull] UriString uri, string scheme)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));
            
            return
                SoftString.Comparer.Equals(uri.Scheme, scheme)
                    ? uri
                    : throw DynamicException.Create("InvalidScheme", $"This resource-provider '{GetType().ToPrettyString()}' requires scheme '{scheme}'.");
        }

        protected UriString ValidateSchemeNotEmpty([NotNull] UriString uri)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));
            
            return
                uri.Scheme
                    ? uri
                    : throw DynamicException.Create("SchemeNotFound", $"Uri '{uri}' does not contain scheme.");
        }
    }

    public static class ResourceHelper
    {
        public static (Stream Stream, ResourceMetadata Metadata) CreateStream(object value)
        {
            // Don't dispose streams. The caller takes care of that.

            switch (value)
            {
                case string s:
                    var streamReader = s.ToStreamReader();
                    return (streamReader.BaseStream, ResourceMetadata.Empty.Add(Serializer, nameof(StreamReader)));
                default:
                    var binaryFormatter = new BinaryFormatter();
                    var memoryStream = new MemoryStream();
                    binaryFormatter.Serialize(memoryStream, value);
                    return (memoryStream, ResourceMetadata.Empty.Add(Serializer, nameof(BinaryFormatter)));
            }
        }

        public static object CreateObject(Stream stream, ResourceMetadata metadata)
        {
            if (metadata.TryGetValue(Serializer, out string serializerName))
            {
                if (serializerName == nameof(BinaryFormatter))
                {
                    var binaryFormatter = new BinaryFormatter();
                    return binaryFormatter.Deserialize(stream);
                }

                if (serializerName == nameof(StreamReader))
                {
                    using (var streamReader = new StreamReader(stream))
                    {
                        return streamReader.ReadToEnd();
                    }
                }

                throw DynamicException.Create("UnsupportedSerializer", $"Cannot deserialize stream because the serializer '{serializerName}' is not supported.");
            }

            throw DynamicException.Create("SerializerNotFound", $"Serializer wasn't specified.");
        }
    }
}
