using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Extensions;
using Reusable.Quickey;

namespace Reusable.IOnymous
{
    public class EmbeddedFileProvider : ResourceProvider
    {
        private readonly Assembly _assembly;

        public EmbeddedFileProvider([NotNull] Assembly assembly, string basePath, IImmutableContainer properties = default)
            : base((properties ?? ImmutableContainer.Empty)
                .SetScheme(UriSchemes.Known.File))
        {
            _assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
            BaseUri = new UriString($"{UriSchemes.Known.File}:{basePath.Replace('.', '/')}");
            Methods =
                MethodCollection
                    .Empty
                    .Add(RequestMethod.Get, GetAsync);
        }

        public EmbeddedFileProvider([NotNull] Assembly assembly, IImmutableContainer properties = default)
            : this(assembly, assembly.GetName().Name.Replace('.', '/'), properties) { }

        public UriString BaseUri { get; }

        #region ResourceProvider

        [ResourceGet]
        public Task<IResource> GetFileAsync(Request request)
        {
            // Embedded resource names are separated by '.' so replace the windows separator.

            var fullUri = BaseUri + request.Uri;
            var fullName = fullUri.Path.Decoded.ToString().Replace('/', '.');

            // Embedded resource names are case sensitive so find the actual name of the resource.
            var actualName = _assembly.GetManifestResourceNames().FirstOrDefault(name => SoftString.Comparer.Equals(name, fullName));

            return
                actualName is null
                    ? DoesNotExist(request).ToTask()
                    : new EmbeddedFile(request.Context.CopyResourceProperties().SetUri(fullUri), () => _assembly.GetManifestResourceStream(actualName)).ToTask<IResource>();
        }

        private Task<IResource> GetAsync(Request request)
        {
            // Embedded resource names are separated by '.' so replace the windows separator.

            var fullUri = BaseUri + request.Uri;
            var fullName = fullUri.Path.Decoded.ToString().Replace('/', '.');

            // Embedded resource names are case sensitive so find the actual name of the resource.
            var actualName = _assembly.GetManifestResourceNames().FirstOrDefault(name => SoftString.Comparer.Equals(name, fullName));

            return
                actualName is null
                    ? DoesNotExist(request).ToTask()
                    : new EmbeddedFile(request.Context.CopyResourceProperties().SetUri(fullUri), () => _assembly.GetManifestResourceStream(actualName)).ToTask<IResource>();
        }

        #endregion
    }

    public class EmbeddedFileProvider<T> : EmbeddedFileProvider
    {
        public static IResourceProvider Default { get; } = new EmbeddedFileProvider(typeof(T).Assembly, ImmutableContainer.Empty);

        public static IResourceProvider Create(string basePath) => new EmbeddedFileProvider(typeof(T).Assembly, basePath);

        public EmbeddedFileProvider(string basePath, IImmutableContainer properties = default) : base(typeof(T).Assembly, basePath, properties) { }
    }

    internal class EmbeddedFile : Resource
    {
        private readonly Func<Stream> _getManifestResourceStream;

        public EmbeddedFile(IImmutableContainer properties, [NotNull] Func<Stream> getManifestResourceStream)
            : base(properties.SetExists(true))
        {
            _getManifestResourceStream = getManifestResourceStream ?? throw new ArgumentNullException(nameof(getManifestResourceStream));
        }

        public override async Task CopyToAsync(Stream stream)
        {
            using (var resourceStream = _getManifestResourceStream())
            {
                await resourceStream.CopyToAsync(stream);
            }
        }
    }
}