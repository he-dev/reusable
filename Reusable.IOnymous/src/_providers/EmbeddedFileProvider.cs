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

        public EmbeddedFileProvider([NotNull] Assembly assembly, IImmutableContainer properties = default)
            : base(
                properties
                    .ThisOrEmpty()
                    .SetScheme(UriSchemes.Known.File)
                    .SetItem(ResourceProviderProperty.SupportsRelativeUri, true)
            )

        {
            _assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
        }


        [ResourceGet]
        public Task<IResource> GetFileAsync(Request request)
        {
            // Embedded resource names are separated by '.' so replace the windows separator.

            var baseUri = Properties.GetItemOrDefault(PropertySelectors.BaseUri);

            var fullUri = baseUri is null ? request.Uri : baseUri + request.Uri.Path;
            var fullName = fullUri.Path.Decoded.ToString().Replace('/', '.');

            // Embedded resource names are case sensitive so find the actual name of the resource.
            var actualName = _assembly.GetManifestResourceNames().FirstOrDefault(name => SoftString.Comparer.Equals(name, fullName));

            return
                actualName is null
                    ? DoesNotExist(request).ToTask()
                    : new EmbeddedFile(request.Context.CopyResourceProperties().SetUri(fullUri), () => _assembly.GetManifestResourceStream(actualName)).ToTask<IResource>();
        }

        [UseType, UseMember]
        [PlainSelectorFormatter]
        public class PropertySelectors : SelectorBuilder<PropertySelectors>
        {
            public static Selector<UriString> BaseUri { get; } = Select(() => BaseUri);
        }
    }

    public class EmbeddedFileProvider<T> : EmbeddedFileProvider
    {
        public static IResourceProvider Default { get; } = new EmbeddedFileProvider(typeof(T).Assembly, ImmutableContainer.Empty);

        public static IResourceProvider Create(string basePath) => new EmbeddedFileProvider(typeof(T).Assembly, ImmutableContainer.Empty.SetItem(PropertySelectors.BaseUri, basePath));

        public EmbeddedFileProvider(IImmutableContainer properties = default) : base(typeof(T).Assembly, properties) { }

        public EmbeddedFileProvider(string baseUri)
            : base(typeof(T).Assembly, ImmutableContainer.Empty.SetItem(PropertySelectors.BaseUri, baseUri)) { }
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