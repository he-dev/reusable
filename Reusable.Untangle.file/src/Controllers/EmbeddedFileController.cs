using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Reusable.Essentials.Extensions;
using Reusable.Essentials;
using Reusable.Translucent.Abstractions;
using Reusable.Translucent.Data;

namespace Reusable.Translucent.Controllers;

public class EmbeddedFileController : ResourceController<FileRequest>
{
    public EmbeddedFileController(string resourceNameRoot, Assembly assembly)
    {
        base.ResourceNameRoot = resourceNameRoot;
        Assembly = assembly;
    }

    private Assembly Assembly { get; }

    public override async Task<Response> ReadAsync(FileRequest request)
    {
        var fullName = NormalizeUri(Path.Combine(ResourceNameRoot ?? string.Empty, request.ResourceName.Peek()));

        // Embedded resource names are case sensitive so find the actual name of the resource.
        var actualName = Assembly.GetManifestResourceNames().FirstOrDefault(name => SoftString.Comparer.Equals(name, fullName));

        if (actualName is { } && Assembly.GetManifestResourceStream(actualName) is {} stream)
        {
            if (request is FileRequest.Stream)
            {
                return Success<FileResponse>(request.ResourceName, Assembly.GetManifestResourceStream(actualName));
            }

            if (request is FileRequest.Text)
            {
                return Success<FileResponse>(request.ResourceName, await stream.ReadTextAsync());
            }
            
            throw DynamicException.Create("InvalidRequest");
        }
        else
        {
            return NotFound<FileResponse>(request.ResourceName);
        }
    }

    // Embedded resource names are separated by '.' so replace the windows separator.
    private static string NormalizeUri(string uri) => Regex.Replace(uri, @"\\|\/", ".");
}

public class EmbeddedFileController<T> : EmbeddedFileController
{
    public EmbeddedFileController(string? resourceNameRoot = default) : base(resourceNameRoot ?? typeof(T).Namespace!, typeof(T).Assembly) { }

    public static IResourceController Default { get; } = new EmbeddedFileController(typeof(T).Namespace!, typeof(T).Assembly);

    public static IResourceController Create(string resourceNameRoot) => new EmbeddedFileController(resourceNameRoot, typeof(T).Assembly);
}