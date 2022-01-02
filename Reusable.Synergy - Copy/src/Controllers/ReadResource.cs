using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Reusable.Essentials;
using Reusable.Essentials.Extensions;

namespace Reusable.Synergy.Controllers;

public abstract class ReadResource<T> : Service<T>
{
    protected ReadResource(Assembly assembly, string name)
    {
        Assembly = assembly;
        Name = name;
    }

    public Assembly Assembly { get; }

    public string Name { get; set; }

    // Embedded resource names are separated by '.' so replace the windows separator.
    public string NameNormalized => Regex.Replace(Name, @"\\|\/", ".");

    protected Stream? FindResource()
    {
        // Embedded resource names are case sensitive so find the actual name of the resource.
        var actualName = Assembly.GetManifestResourceNames().FirstOrDefault(name => name.EndsWith(NameNormalized, StringComparison.OrdinalIgnoreCase));

        return actualName is { } ? Assembly.GetManifestResourceStream(actualName) : default;
    }
}

public static class ReadResource
{
    public class Text : ReadResource<string>
    {
        public Text(Assembly assembly, string name) : base(assembly, name) { }

        public override async Task<string> InvokeAsync()
        {
            if (FindResource() is { } stream)
            {
                await using (stream)
                {
                    return await stream.ReadTextAsync();
                }
            }
            else
            {
                throw DynamicException.Create("ResourceNotFound", $"There is no such file as '{NameNormalized}'.");
            }
        }
    }

    public class Stream : ReadResource<System.IO.Stream>
    {
        public Stream(Assembly assembly, string name) : base(assembly, name) { }

        public override Task<System.IO.Stream> InvokeAsync()
        {
            if (FindResource() is { } stream)
            {
                return stream.ToTask();
            }
            else
            {
                throw DynamicException.Create("ResourceNotFound", $"There is no such file as '{NameNormalized}'.");
            }
        }
    }
}