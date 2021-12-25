using System;
using System.Threading.Tasks;
using Reusable.Octopus;
using Reusable.Octopus.Data;
using Reusable.Translucent.Data;

namespace Reusable.Translucent;

public static class ResourceHelpers
{
    public static Task<Response> ReadFileAsync<T>(this IResource resource, string path, Action<T>? configure = default) where T : FileRequest, new()
    {
        return resource.ReadAsync(path, default, configure);
    }

    public static async Task<string> ReadTextFileAsync(this IResource resource, string path, Action<FileRequest.Text>? configure = default)
    {
        using var file = await resource.ReadAsync(path, default, configure);
        return (string)file.Body.Peek(); // await file.DeserializeTextAsync();
    }

    public static string ReadTextFile(this IResource resources, string path, Action<FileRequest.Text>? configure = default)
    {
        using var file = resources.ReadAsync(path, default, configure).GetAwaiter().GetResult();
        return (string)file.Body.Peek(); // .DeserializeTextAsync().GetAwaiter().GetResult();
    }

    public static async Task WriteTextFileAsync(this IResource resources, string path, string value, Action<FileRequest.Text>? configure = default)
    {
        using (await resources.CreateAsync(path, value, configure)) { }
    }

    public static async Task WriteFileAsync(this IResource resources, string path, BodyStreamFunc bodyStream, Action<FileRequest.Stream>? configure = default)
    {
        using (await resources.CreateAsync(path, bodyStream, configure)) { }
    }

    public static async Task DeleteFileAsync(this IResource resources, string path, Action<FileRequest>? configure = default)
    {
        using (await resources.DeleteAsync(path, default, configure)) { }
    }
}