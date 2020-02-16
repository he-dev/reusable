using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Reusable.Extensions;
using Reusable.Translucent.Abstractions;
using Reusable.Translucent.Data;

namespace Reusable.Translucent.Controllers
{
    public class EmbeddedFileController : ResourceController<FileRequest>
    {
        private readonly Assembly _assembly;

        public EmbeddedFileController(string baseUri, Assembly assembly) : base(baseUri)
        {
            _assembly = assembly;
        }

        public override Task<Response> ReadAsync(FileRequest request)
        {
            var fullName = NormalizeUri(Path.Combine(BaseUri ?? string.Empty, request.ResourceName));

            // Embedded resource names are case sensitive so find the actual name of the resource.
            var actualName = _assembly.GetManifestResourceNames().FirstOrDefault(name => SoftString.Comparer.Equals(name, fullName));

            return
                actualName is {}
                    ? Success<FileResponse>(request.ResourceName, _assembly.GetManifestResourceStream(actualName)).ToTask<Response>()
                    : NotFound<FileResponse>(request.ResourceName).ToTask<Response>();
        }

        // Embedded resource names are separated by '.' so replace the windows separator.
        private static string NormalizeUri(string uri) => Regex.Replace(uri, @"\\|\/", ".");
    }

    public class EmbeddedFileController<T> : EmbeddedFileController
    {
        public EmbeddedFileController(string? baseUri = default) : base(baseUri ?? typeof(T).Namespace, typeof(T).Assembly) { }

        public static IResourceController Default { get; } = new EmbeddedFileController(typeof(T).Namespace, typeof(T).Assembly);

        public static IResourceController Create(string basePath) => new EmbeddedFileController(basePath, typeof(T).Assembly);
    }
}