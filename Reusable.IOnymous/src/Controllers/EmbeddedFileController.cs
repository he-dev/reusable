using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Extensions;
using Reusable.Quickey;

namespace Reusable.IOnymous.Controllers
{
    public class EmbeddedFileController : ResourceController
    {
        private readonly Assembly _assembly;

        public EmbeddedFileController([NotNull] Assembly assembly, IImmutableContainer properties = default)
            : base(
                properties
                    .ThisOrEmpty()
                    .UpdateItem(ResourceControllerProperties.Schemes, s => s.Add(UriSchemes.Known.File))
                    .SetItem(ResourceControllerProperties.SupportsRelativeUri, true)
            )

        {
            _assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
        }


        [ResourceGet]
        public Task<IResource> GetFileAsync(Request request)
        {
            // Embedded resource names are separated by '.' so replace the windows separator.

            var baseUri = Properties.GetItemOrDefault(EmbeddedFileControllerProperties.BaseUri);

            var fullUri = baseUri is null ? request.Uri : baseUri + request.Uri.Path;
            var fullName = fullUri.Path.Decoded.ToString().Replace('/', '.');

            // Embedded resource names are case sensitive so find the actual name of the resource.
            var actualName = _assembly.GetManifestResourceNames().FirstOrDefault(name => SoftString.Comparer.Equals(name, fullName));

            return
                actualName is null
                    ? DoesNotExist(request).ToTask()
                    : new EmbeddedFile(request.Metadata.Copy<ResourceProperties>().SetItem(ResourceProperties.Uri, fullUri), () => _assembly.GetManifestResourceStream(actualName)).ToTask<IResource>();
        }
    }

    [UseType, UseMember]
    [PlainSelectorFormatter]
    public class EmbeddedFileControllerProperties : SelectorBuilder<EmbeddedFileControllerProperties>
    {
        public static Selector<UriString> BaseUri { get; } = Select(() => BaseUri);
    }

    public class EmbeddedFileController<T> : EmbeddedFileController
    {
        public static IResourceController Default { get; } = new EmbeddedFileController(typeof(T).Assembly, ImmutableContainer.Empty);

        public static IResourceController Create(string basePath) => new EmbeddedFileController(typeof(T).Assembly, ImmutableContainer.Empty.SetItem(EmbeddedFileControllerProperties.BaseUri, basePath));

        public EmbeddedFileController(IImmutableContainer properties = default) : base(typeof(T).Assembly, properties) { }

        public EmbeddedFileController(string baseUri)
            : base(typeof(T).Assembly, ImmutableContainer.Empty.SetItem(EmbeddedFileControllerProperties.BaseUri, baseUri)) { }
    }

    internal class EmbeddedFile : Resource
    {
        private readonly Func<Stream> _getManifestResourceStream;

        public EmbeddedFile(IImmutableContainer properties, [NotNull] Func<Stream> getManifestResourceStream)
            : base(properties.SetItem(ResourceProperties.Exists, true))
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