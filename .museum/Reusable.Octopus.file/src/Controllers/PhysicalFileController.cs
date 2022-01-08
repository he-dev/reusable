using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Essentials;
using Reusable.Essentials.Extensions;
using Reusable.Octopus.Abstractions;
using Reusable.Octopus.Data;
using Reusable.Octopus.Extensions;

namespace Reusable.Octopus.Controllers;

[PublicAPI]
public class PhysicalFileController : ResourceController<FileRequest>
{
    public PhysicalFileController() : base(new[] { "file" }) { }

    protected override Task<Response> ReadAsync(FileRequest request)
    {
        ResolvePath(request);
        
        if (File.Exists(request.ResourceName.Value))
        {
            var fileStream = new FileStream(request, FileMode.Open, FileAccess.Read, FileShare.Read);
            return Success<FileResponse>(request, fileStream).ToTask();
        }
        else
        {
            return NotFound<FileResponse>(request.ResourceName.Value).ToTask();
        }
    }

    protected override async Task<Response> CreateAsync(FileRequest request)
    {
        ResolvePath(request);

        if (request.TryGetItem<FileMode>(out var fileMode) && fileMode == FileMode.Append)
        {
            var fileStream = new FileStream(request.ResourceName.Value, FileMode.Append, FileAccess.Write);

            return Success<FileResponse>(request.ResourceName.Value, fileStream);
        }
        else
        {
            if (request.Data.Value is not Stream stream)
            {
                throw DynamicException.Create("InvalidDataType", $"Expected '{nameof(Stream)}' but received '{request.Data.Value.GetType().ToPrettyString()}'.");
            }

            await using var fileStream = new FileStream(request.ResourceName.Value, FileMode.CreateNew, FileAccess.Write);
            await stream.Rewind().CopyToAsync(fileStream);
            await fileStream.FlushAsync();

            return Success<FileResponse>(request.ResourceName.Value);
        }
    }

    protected override async Task<Response> UpdateAsync(FileRequest request) => await CreateAsync(request);

    protected override Task<Response> DeleteAsync(FileRequest request)
    {
        ResolvePath(request);
        File.Delete(request.ResourceName.Value);
        return Success<FileResponse>(request.ResourceName.Value).ToTask();
    }

    private void ResolvePath(FileRequest request)
    {
        var currentName = request.ResourceName.Value;

        var fullName =
            Path.IsPathRooted(currentName)
                ? currentName
                : ResourceNameRoot is { }
                    ? Path.Combine(ResourceNameRoot, currentName)
                    : currentName;

        request.ResourceName.Push(fullName);
    }
}