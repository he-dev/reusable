using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Extensions;
using Reusable.Quickey;

namespace Reusable.Translucent.Controllers
{
    public class EmbeddedFileController : ResourceController
    {
        private readonly Assembly _assembly;

        public EmbeddedFileController(string? id, Assembly assembly, string basePath) : base(id, basePath, UriSchemes.Known.File)
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
                actualName is null
                    ? NotFound().ToTask()
                    : OK(_assembly.GetManifestResourceStream(actualName)).ToTask();
        }
    }

    public class EmbeddedFileController<T> : EmbeddedFileController
    {
        public static IResourceController Default { get; } = new EmbeddedFileController(default, typeof(T).Assembly, typeof(T).Namespace);

        public static IResourceController Create(string? id, string basePath) => new EmbeddedFileController(id, typeof(T).Assembly, basePath);

        public EmbeddedFileController(string? id, string? baseUri = default) : base(id, typeof(T).Assembly, baseUri ?? typeof(T).Namespace) { }
    }
}