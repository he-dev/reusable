using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.Translucent.Abstractions;
using Reusable.Translucent.Data;
using Reusable.Translucent.Extensions;

namespace Reusable.Translucent.Controllers
{
    [PublicAPI]
    public class PhysicalFileResourceController : ResourceController<FileRequest>
    {
        public PhysicalFileResourceController(string? baseUri = default) : base(baseUri) { }

        public override Task<Response> ReadAsync(FileRequest request)
        {
            var path = CreatePath(request.ResourceName);

            return
                File.Exists(path)
                    ? Success<FileResponse>(request.ResourceName, File.OpenRead(path)).ToTask()
                    : NotFound<FileResponse>(request.ResourceName).ToTask();
        }

        public override async Task<Response> CreateAsync(FileRequest request)
        {
            var path = CreatePath(request.ResourceName);

            using var fileStream = new FileStream(path, FileMode.CreateNew, FileAccess.Write);
            using var body = await request.CreateBodyStreamAsync();
            await body.Rewind().CopyToAsync(fileStream);
            await fileStream.FlushAsync();

            return Success<FileResponse>(request.ResourceName);
        }

        public override Task<Response> DeleteAsync(FileRequest request)
        {
            var path = CreatePath(request.ResourceName);
            File.Delete(path);
            return Success<FileResponse>(request.ResourceName).ToTask<Response>();
        }

        private string CreatePath(string path)
        {
            //var path = uri.ToUnc();

            return
                Path.IsPathRooted(path)
                    ? path
                    : BaseUri is {} basePath
                        ? Path.Combine(basePath, path)
                        : path;
        }
    }
    
    
}