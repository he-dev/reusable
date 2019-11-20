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
        public EmbeddedFileController(Assembly assembly, UriString baseUri, IImmutableContainer? properties = default)
            : base(
                new SoftString[] { UriSchemes.Known.File },
                baseUri,
                properties
                    .ThisOrEmpty()
                    .SetItem(Assembly, assembly)
            ) { }

        [ResourceGet]
        public Task<Response> GetFileAsync(Request request)
        {
            // Embedded resource names are separated by '.' so replace the windows separator.

            var baseUri = Properties.GetItemOrDefault(BaseUri);
            var fullUri = baseUri is null ? request.Uri : baseUri + request.Uri.Path;
            var fullName = fullUri.Path.Decoded.ToString().Replace('/', '.');

            // Embedded resource names are case sensitive so find the actual name of the resource.
            var actualName = Properties.GetItem(Assembly).GetManifestResourceNames().FirstOrDefault(name => SoftString.Comparer.Equals(name, fullName));

            return
                actualName is null
                    ? NotFound().ToTask()
                    : OK(Properties.GetItem(Assembly).GetManifestResourceStream(actualName)).ToTask();
        }

        #region Properties

        private static readonly From<EmbeddedFileController>? This;

        public static Selector<Assembly> Assembly { get; } = This.Select(() => Assembly);

        #endregion
    }

    public class EmbeddedFileController<T> : EmbeddedFileController
    {
        public static IResourceController Default { get; } = new EmbeddedFileController(typeof(T).Assembly, typeof(T).Namespace);

        public static IResourceController Create(string basePath) => new EmbeddedFileController(typeof(T).Assembly, basePath);

        public EmbeddedFileController(string? baseUri = default, IImmutableContainer? properties = default) : base(typeof(T).Assembly, baseUri ?? typeof(T).Namespace, properties) { }
    }
}