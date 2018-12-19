using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Reusable.IOnymous
{
    public class EmbeddedFileProvider : ResourceProvider
    {
        private readonly Assembly _assembly;

        public EmbeddedFileProvider([NotNull] Assembly assembly, ResourceMetadata metadata = null)
            : base(
                (metadata ?? ResourceMetadata.Empty)
                    .Add(ResourceMetadataKeys.CanGet, true)
            )
        {
            _assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
            //BaseUri = new Uri(_assembly.GetName().Name.Replace('.', Path.DirectorySeparatorChar));
            var assemblyName = _assembly.GetName().Name.Replace('.', '/');
            BaseUri = new UriString($"file:{assemblyName}");
        }

        public UriString BaseUri { get; }

        #region ResourceProvider

        public override Task<IResourceInfo> GetAsync(UriString uri, ResourceMetadata metadata = null)
        {
            ValidateSchemeNotEmpty(uri);

            // Embedded resource names are separated by '.' so replace the windows separator.

            var fullUri = BaseUri + uri.Path.Value;
            var fullName = fullUri.Path.Value.Replace('/', '.');

            // Embedded resource names are case sensitive so find the actual name of the resource.
            var actualName = _assembly.GetManifestResourceNames().FirstOrDefault(name => SoftString.Comparer.Equals(name, fullName));
            var getManifestResourceStream = actualName is null ? default(Func<Stream>) : () => _assembly.GetManifestResourceStream(actualName);

            return Task.FromResult<IResourceInfo>(new EmbeddedFileInfo(UndoConvertPath(fullName), getManifestResourceStream));
        }

        public override Task<IResourceInfo> PutAsync(UriString uri, Stream data, ResourceMetadata metadata = null)
        {
            throw new NotSupportedException($"{nameof(EmbeddedFileProvider)} does not support value serialization.");
        }

        public override Task<IResourceInfo> DeleteAsync(UriString uri, ResourceMetadata metadata = null)
        {
            throw new NotSupportedException($"{nameof(EmbeddedFileProvider)} does not support value deletion.");
        }

        #endregion

        // Convert path back to windows format but the last '.' - this is the file extension.
        private static string UndoConvertPath(string path) => Regex.Replace(path, @"\.(?=.*?\.)", Path.DirectorySeparatorChar.ToString());
    }

    public static class EmbeddedFileProvider<T>
    {
        public static IResourceProvider Default { get; } = new EmbeddedFileProvider(typeof(T).Assembly);
    }
    
    internal class EmbeddedFileInfo : ResourceInfo
    {
        private readonly Func<Stream> _getManifestResourceStream;

        public EmbeddedFileInfo(string uri, Func<Stream> getManifestResourceStream) : base(uri)
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
            AssertExists();

            using (var resourceStream = _getManifestResourceStream())
            {
                await resourceStream.CopyToAsync(stream);
            }
        }

        public override async Task<object> DeserializeAsync(Type targetType)
        {
            AssertExists();

            using (var resourceStream = _getManifestResourceStream())
            using (var streamReader = new StreamReader(resourceStream))
            {
                return await streamReader.ReadToEndAsync();
            }
        }
    }
}