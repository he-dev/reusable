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
            BaseUri = new Uri(_assembly.GetName().Name.Replace('.', '/'));
        }

        public Uri BaseUri { get; }

        #region IFileProvider

        public override Task<IResourceInfo> GetAsync(UriString uri, ResourceMetadata metadata = null)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));

            ValidateScheme(uri);

            // Embedded resouce names are separated by '.' so replace the windows separator.
            //var fullName = new Uri(BaseUri, uri).LocalPath.Replace(Path.DirectorySeparatorChar, '.');
            var fullName = new Uri(BaseUri, uri).LocalPath.Replace('/', '.');

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
}