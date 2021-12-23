using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.Translucent.Abstractions;
using Reusable.Translucent.Data;
using Reusable.Translucent.Extensions;

namespace Reusable.Translucent.Controllers;

[PublicAPI]
public class PhysicalFileController : ResourceController<FileRequest>
{
    public override async Task<Response> ReadAsync(FileRequest request)
    {
        ResolvePath(request);

        if (File.Exists(request.ResourceName.Peek()))
        {
            if (request is FileRequest.Text)
            {
                return Success<FileResponse>(request.ResourceName, await File.ReadAllTextAsync(request.ResourceName.Peek()));
            }

            if (request is FileRequest.Stream)
            {
                return Success<FileResponse>(request.ResourceName, File.OpenRead(request.ResourceName.Peek()));
            }

            throw DynamicException.Create("UnsupportedFileRequest", $"File request type: {request.GetType().ToPrettyString()}");
        }
        else
        {
            return NotFound<FileResponse>(request.ResourceName);
        }
    }

    public override async Task<Response> CreateAsync(FileRequest request)
    {
        ResolvePath(request);

        if (request is FileRequest.Text && request.Body.Peek() is string text)
        {
            await File.WriteAllTextAsync(request.ResourceName.Peek(), text);

            return Success<FileResponse>(request.ResourceName);
        }

        if (request is FileRequest.Stream && request.Body.Peek() is Stream stream)
        {
            await using var fileStream = new FileStream(request.ResourceName.Peek(), FileMode.CreateNew, FileAccess.Write);
            //await using var body = await request.CreateBodyStreamAsync();
            await stream.Rewind().CopyToAsync(fileStream);
            await fileStream.FlushAsync();

            return Success<FileResponse>(request.ResourceName);
        }

        throw DynamicException.Create("UnsupportedFileRequest", $"File request type: {request.GetType().ToPrettyString()}");
    }

    public override Task<Response> DeleteAsync(FileRequest request)
    {
        ResolvePath(request);
        File.Delete(request.ResourceName.Peek());
        return Success<FileResponse>(request.ResourceName).ToTask();
    }

    private void ResolvePath(FileRequest request)
    {
        var currentName = request.ResourceName.Peek();

        var fullName =
            Path.IsPathRooted(currentName)
                ? currentName
                : ResourceNameRoot is { }
                    ? Path.Combine(ResourceNameRoot, currentName)
                    : currentName;
        
        request.ResourceName.Push(fullName);
    }
}