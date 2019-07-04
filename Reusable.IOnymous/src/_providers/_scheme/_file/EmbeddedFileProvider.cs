using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Quickey;

namespace Reusable.IOnymous
{
    public class EmbeddedFileProvider : FileProvider
    {
        private readonly Assembly _assembly;

        public EmbeddedFileProvider([NotNull] Assembly assembly, IImmutableSession metadata = default)
            : base((metadata ?? ImmutableSession.Empty).SetScheme(ResourceSchemes.IOnymous).SetItem(From<IProviderMeta>.Select(x => x.AllowRelativeUri), true))
        {
            _assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
            var assemblyName = _assembly.GetName().Name.Replace('.', '/');
            BaseUri = new UriString($"{DefaultScheme}:{assemblyName}");
        }

        public UriString BaseUri { get; }

        #region ResourceProvider

        protected override Task<IResourceInfo> GetAsyncInternal(UriString uri, IImmutableSession metadata)
        {
            //ValidateFormatNotNull(this, uri, metadata);

            // Embedded resource names are separated by '.' so replace the windows separator.

            var fullUri = BaseUri + uri;
            var fullName = fullUri.Path.Decoded.ToString().Replace('/', '.');

            // Embedded resource names are case sensitive so find the actual name of the resource.
            var actualName = _assembly.GetManifestResourceNames().FirstOrDefault(name => SoftString.Comparer.Equals(name, fullName));
            var getManifestResourceStream = actualName is null ? default(Func<Stream>) : () => _assembly.GetManifestResourceStream(actualName);

            return Task.FromResult<IResourceInfo>(new EmbeddedFileInfo(fullUri, metadata.GetItemOrDefault(From<IResourceMeta>.Select(y => y.Format)), getManifestResourceStream));
        }

        #endregion
    }

    public static class EmbeddedFileProvider<T>
    {
        public static IResourceProvider Default { get; } = new EmbeddedFileProvider(typeof(T).Assembly, ImmutableSession.Empty);
    }

    internal class EmbeddedFileInfo : ResourceInfo
    {
        private readonly Func<Stream> _getManifestResourceStream;

        public EmbeddedFileInfo(string uri, MimeType format, Func<Stream> getManifestResourceStream)
            : base(uri, ImmutableSession.Empty.SetItem(From<IResourceMeta>.Select(x => x.Format), format))
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


        protected override async Task CopyToAsyncInternal(Stream stream)
        {
            using (var resourceStream = _getManifestResourceStream())
            {
                await resourceStream.CopyToAsync(stream);
            }
        }
    }
}