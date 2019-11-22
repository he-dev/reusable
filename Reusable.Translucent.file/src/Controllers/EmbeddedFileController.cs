using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Reusable.Extensions;
using Reusable.Translucent.Annotations;

namespace Reusable.Translucent.Controllers
{
    [Handles(typeof(FileRequest))]
    public class EmbeddedFileController : ResourceController
    {
        private readonly Assembly _assembly;

        public EmbeddedFileController(string? id, string basePath, Assembly assembly) : base(id, basePath, UriSchemes.Known.File)
        {
            _assembly = assembly;
        }

        [ResourceGet]
        public Task<Response> GetFileAsync(Request request)
        {
            // Embedded resource names are separated by '.' so replace the windows separator.

            var fullUri = BaseUri is {} baseUri ? baseUri + request.Uri.Path : request.Uri;
            var fullName = fullUri.Path.Decoded.ToString().Replace('/', '.');

            // Embedded resource names are case sensitive so find the actual name of the resource.
            var actualName = _assembly.GetManifestResourceNames().FirstOrDefault(name => SoftString.Comparer.Equals(name, fullName));

            return
                actualName is {}
                    ? OK<FileResponse>(_assembly.GetManifestResourceStream(actualName)).ToTask()
                    : NotFound<FileResponse>().ToTask();
        }
    }

    public class EmbeddedFileController<T> : EmbeddedFileController
    {
        public EmbeddedFileController(string? id, string? baseUri = default) : base(id, baseUri ?? typeof(T).Namespace, typeof(T).Assembly) { }

        public static IResourceController Default { get; } = new EmbeddedFileController(default, typeof(T).Namespace, typeof(T).Assembly);

        public static IResourceController Create(string? id, string basePath) => new EmbeddedFileController(id, basePath, typeof(T).Assembly);
    }
}