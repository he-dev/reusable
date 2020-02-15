using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Reusable.Extensions;
using Reusable.Translucent.Abstractions;
using Reusable.Translucent.Annotations;
using Reusable.Translucent.Data;

namespace Reusable.Translucent.Controllers
{
    [Handles(typeof(FileRequest))]
    public class EmbeddedFileController : ResourceController
    {
        private readonly Assembly _assembly;

        public EmbeddedFileController(ControllerName name, string baseUri, Assembly assembly) : base(name, baseUri)
        {
            _assembly = assembly;
        }

        [ResourceGet]
        public Task<Response> GetFileAsync(Request request)
        {
            var fullName = NormalizeUri(Path.Combine(BaseUri ?? string.Empty, request.ResourceName));

            // Embedded resource names are case sensitive so find the actual name of the resource.
            var actualName = _assembly.GetManifestResourceNames().FirstOrDefault(name => SoftString.Comparer.Equals(name, fullName));

            return
                actualName is {}
                    ? OK<FileResponse>(_assembly.GetManifestResourceStream(actualName)).ToTask<Response>()
                    : NotFound<FileResponse>().ToTask<Response>();
        }

        // Embedded resource names are separated by '.' so replace the windows separator.
        private static string NormalizeUri(string uri) => Regex.Replace(uri, @"\\|\/", ".");
    }

    public class EmbeddedFileController<T> : EmbeddedFileController
    {
        public EmbeddedFileController(ControllerName name, string? baseUri = default) : base(name, baseUri ?? typeof(T).Namespace, typeof(T).Assembly) { }

        public static IResourceController Default { get; } = new EmbeddedFileController(ControllerName.Empty, typeof(T).Namespace, typeof(T).Assembly);

        public static IResourceController Create(ControllerName controllerName, string basePath) => new EmbeddedFileController(controllerName, basePath, typeof(T).Assembly);
    }
}