using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Reusable.Essentials;
using Reusable.Essentials.Extensions;
using Reusable.Octopus.Abstractions;
using Reusable.Octopus.Data;

namespace Reusable.Octopus.Controllers;

public class EmbeddedFileController : ResourceController<FileRequest>
{
    public EmbeddedFileController(string resourceNameRoot, Assembly assembly) : base(new[] { "file" })
    {
        base.ResourceNameRoot = resourceNameRoot;
        Assembly = assembly;
    }

    private Assembly Assembly { get; }

    protected override Task<Response> ReadAsync(FileRequest request)
    {
        NormalizeUri(request);

        // Embedded resource names are case sensitive so find the actual name of the resource.
        var actualName = Assembly.GetManifestResourceNames().FirstOrDefault(name => SoftString.Comparer.Equals(name, request.ResourceName.Value));

        if (actualName is { } && Assembly.GetManifestResourceStream(actualName) is { } stream)
        {
            return Success<FileResponse>(request.ResourceName.Value, stream).ToTask();
        }
        else
        {
            return NotFound<FileResponse>(request.ResourceName.Value).ToTask();
        }
    }

    // Embedded resource names are separated by '.' so replace the windows separator.
    private void NormalizeUri(Request request)
    {
        var fullName = Path.Combine(ResourceNameRoot ?? string.Empty, request.ResourceName.Value);
        if (Regex.Replace(fullName, @"\\|\/", ".") is var normalized && !SoftString.Comparer.Equals(fullName, normalized))
        {
            request.ResourceName.Push(normalized);
        }
    }
}

public class EmbeddedFileController<T> : EmbeddedFileController
{
    public EmbeddedFileController(string? resourceNameRoot = default) : base(resourceNameRoot ?? typeof(T).Namespace!, typeof(T).Assembly) { }

    public static IResourceController Default { get; } = new EmbeddedFileController(typeof(T).Namespace!, typeof(T).Assembly);

    public static IResourceController Create(string resourceNameRoot) => new EmbeddedFileController(resourceNameRoot, typeof(T).Assembly);
}