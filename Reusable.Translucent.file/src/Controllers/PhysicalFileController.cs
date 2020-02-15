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
        public PhysicalFileResourceController(ControllerName name, string? baseUri = default) : base(name, baseUri) { }

        public override Task<Response> ReadAsync(FileRequest request)
        {
            var path = CreatePath(request.ResourceName);

            return
                File.Exists(path)
                    ? Success<FileResponse>(File.OpenRead(path)).ToTask<Response>()
                    : NotFound<FileResponse>().ToTask<Response>();
        }

        public override async Task<Response> CreateAsync(FileRequest request)
        {
            var path = CreatePath(request.ResourceName);

            using var fileStream = new FileStream(path, FileMode.CreateNew, FileAccess.Write);
            using var body = await request.CreateBodyStreamAsync();
            await body.Rewind().CopyToAsync(fileStream);
            await fileStream.FlushAsync();

            return Success<FileResponse>();
        }

        public override Task<Response> DeleteAsync(FileRequest request)
        {
            var path = CreatePath(request.ResourceName);
            File.Delete(path);
            return Success<FileResponse>().ToTask<Response>();
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