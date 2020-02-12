using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Reusable.Extensions;
using Reusable.Translucent.Annotations;
using Reusable.Translucent.Data;

namespace Reusable.Translucent.Controllers
{
    [Handles(typeof(FileRequest))]
    public class EmbeddedFileController : ResourceController
    {
        private readonly Assembly _assembly;

        public EmbeddedFileController(ControllerName controllerName, string basePath, Assembly assembly) : base(controllerName, basePath)
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
        public EmbeddedFileController(ControllerName controllerName, string? baseUri = default) : base(controllerName, baseUri ?? typeof(T).Namespace, typeof(T).Assembly) { }

        public static IResourceController Default { get; } = new EmbeddedFileController(ControllerName.Empty, typeof(T).Namespace, typeof(T).Assembly);

        public static IResourceController Create(ControllerName controllerName, string basePath) => new EmbeddedFileController(controllerName, basePath, typeof(T).Assembly);
    }
}