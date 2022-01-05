using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Essentials;
using Reusable.Essentials.Extensions;
using Reusable.Synergy.Requests;

namespace Reusable.Synergy.Controllers;

public class EmbeddedResourceService : Service
{
    public EmbeddedResourceService(Assembly assembly)
    {
        Assembly = assembly;
        MustSucceed = true;
    }

    private Assembly Assembly { get; }

    public override async Task<object> InvokeAsync(IRequest request)
    {
        if (request is not IReadFile file)
        {
            throw DynamicException.Create("UnknownRequest", $"{request.GetType().ToPrettyString()} is not supported by this {nameof(EmbeddedResourceService)}.");
        }

        var name = Normalize(file.Name);

        if (FindResource(name) is not { } stream)
        {
            return
                MustSucceed
                    ? throw DynamicException.Create("EmbeddedResourceNotFound", $"There is no such file as '{file.Name}'.")
                    : await InvokeNext(request);
        }

        if (request is ReadFile<string>)
        {
            await using (stream)
            {
                return await stream.ReadTextAsync();
            }
        }

        return stream;
    }

    // Embedded resource names are separated by '.' so replace the windows separator.
    private static string Normalize(string name) => Regex.Replace(name, @"\\|\/", ".");

    private Stream? FindResource(string name)
    {
        // Embedded resource names are case sensitive so find the actual name of the resource.
        var actualName = Assembly.GetManifestResourceNames().FirstOrDefault(current => current.EndsWith(name, StringComparison.OrdinalIgnoreCase));

        return actualName is { } ? Assembly.GetManifestResourceStream(actualName) : default;
    }
}

public class EmbeddedResourceService<T> : EmbeddedResourceService
{
    public EmbeddedResourceService() : base(typeof(T).Assembly) { }
}