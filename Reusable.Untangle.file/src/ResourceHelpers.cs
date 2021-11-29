using System;
using System.IO;
using System.Threading.Tasks;
using Reusable.Translucent.Data;
using Reusable.Translucent.Extensions;

namespace Reusable.Translucent
{
    public static class ResourceHelpers
    {
        public static Task<Response> GetFileAsync(this IResource resources, string path, Action<FileRequest>? configureRequest = default)
        {
            return resources.ReadAsync(path, default, configureRequest);
        }

        public static async Task<string> ReadTextFileAsync(this IResource resource, string path, Action<FileRequest>? configureRequest = default)
        {
            using var file = await resource.GetFileAsync(path, configureRequest);
            return await file.DeserializeTextAsync();
        }

        public static string ReadTextFile(this IResource resources, string path, Action<FileRequest>? configureRequest = default)
        {
            using var file = resources.GetFileAsync(path, configureRequest).GetAwaiter().GetResult();
            return file.DeserializeTextAsync().GetAwaiter().GetResult();
        }

        public static async Task WriteTextFileAsync(this IResource resources, string path, string value, Action<FileRequest>? configureRequest = default)
        {
            using (await resources.CreateAsync(path, value, configureRequest)) { }
        }

        public static async Task WriteFileAsync(this IResource resources, string path, CreateBodyStreamDelegate createBodyStream, Action<FileRequest>? configureRequest = default)
        {
            using (await resources.CreateAsync(path, createBodyStream, configureRequest)) { }
        }

        public static async Task DeleteFileAsync(this IResource resources, string path, Action<FileRequest>? configureRequest = default)
        {
            using (await resources.DeleteAsync(path, default, configureRequest)) { }
        }
    }
    
    
}