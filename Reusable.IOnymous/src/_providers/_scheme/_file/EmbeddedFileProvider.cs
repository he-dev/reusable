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
    public class EmbeddedFileProvider : ResourceProvider
    {
        private readonly Assembly _assembly;

        public EmbeddedFileProvider([NotNull] Assembly assembly, IImmutableContainer properties = default)
            : base((properties ?? ImmutableContainer.Empty)
                .SetScheme(UriSchemes.Custom.IOnymous)
                .SetItem(ResourceProviderProperty.AllowRelativeUri, true))
        {
            _assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
            var assemblyName = _assembly.GetName().Name.Replace('.', '/');
            BaseUri = new UriString($"{UriSchemes.Known.File}:{assemblyName}");
            Methods =
                MethodDictionary
                    .Empty
                    .Add(RequestMethod.Get, GetAsync);
        }

        public UriString BaseUri { get; }

        #region ResourceProvider

        private Task<IResource> GetAsync(Request request)
        {
            //ValidateFormatNotNull(this, uri, metadata);

            // Embedded resource names are separated by '.' so replace the windows separator.

            var fullUri = BaseUri + request.Uri;
            var fullName = fullUri.Path.Decoded.ToString().Replace('/', '.');

            // Embedded resource names are case sensitive so find the actual name of the resource.
            var actualName = _assembly.GetManifestResourceNames().FirstOrDefault(name => SoftString.Comparer.Equals(name, fullName));
            var getManifestResourceStream = actualName is null ? default(Func<Stream>) : () => _assembly.GetManifestResourceStream(actualName);

            return Task.FromResult<IResource>(new EmbeddedFile(request.Properties.Copy(Resource.Property.This).SetUri(fullUri), getManifestResourceStream));
        }

        #endregion
    }

    public static class EmbeddedFileProvider<T>
    {
        public static IResourceProvider Default { get; } = new EmbeddedFileProvider(typeof(T).Assembly, ImmutableContainer.Empty);
    }

    internal class EmbeddedFile : Resource
    {
        private readonly Func<Stream> _getManifestResourceStream;

        public EmbeddedFile(IImmutableContainer properties, Func<Stream> getManifestResourceStream)
            : base(properties.SetExists(!(getManifestResourceStream is null)))
        {
            _getManifestResourceStream = getManifestResourceStream;
        }

//        public override long? Length
//        {
//            get
//            {
//                using (var stream = _getManifestResourceStream?.Invoke())
//                {
//                    return stream?.Length;
//                }
//            }
//        }


        public override async Task CopyToAsync(Stream stream)
        {
            using (var resourceStream = _getManifestResourceStream())
            {
                await resourceStream.CopyToAsync(stream);
            }
        }
    }
}