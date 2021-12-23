using System;
using System.IO;
using System.Threading.Tasks;
using Reusable.Translucent.Data;
using Reusable.Translucent.Extensions;

namespace Reusable.Translucent;

public static class ResourceHelpers
{
    public static Task<Response> GetFileAsync<T>(this IResource resource, string path, Action<T>? configure = default) where T : FileRequest, new()
    {
        return resource.ReadAsync(path, default, configure);
    }

    public static async Task<string> ReadTextFileAsync(this IResource resource, string path, Action<FileRequest.Text>? configure = default)
    {
        using var file = await resource.GetFileAsync(path, configure);
        return (string)file.Body.Peek(); // await file.DeserializeTextAsync();
    }

    public static string ReadTextFile(this IResource resources, string path, Action<FileRequest.Text>? configure = default)
    {
        using var file = resources.GetFileAsync(path, configure).GetAwaiter().GetResult();
        return (string)file.Body.Peek(); // .DeserializeTextAsync().GetAwaiter().GetResult();
    }

    public static async Task WriteTextFileAsync(this IResource resources, string path, string value, Action<FileRequest>? configure = default)
    {
        using (await resources.CreateAsync(path, value, configure)) { }
    }

    public static async Task WriteFileAsync(this IResource resources, string path, BodyStreamFunc bodyStream, Action<FileRequest>? configure = default)
    {
        using (await resources.CreateAsync(path, bodyStream, configure)) { }
    }

    public static async Task DeleteFileAsync(this IResource resources, string path, Action<FileRequest>? configure = default)
    {
        using (await resources.DeleteAsync(path, default, configure)) { }
    }
}