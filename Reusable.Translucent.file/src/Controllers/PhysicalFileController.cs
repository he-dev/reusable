using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.Translucent.Annotations;
using Reusable.Translucent.Data;
using Reusable.Translucent.Extensions;

namespace Reusable.Translucent.Controllers
{
    [PublicAPI]
    [Handles(typeof(FileRequest))]
    public class PhysicalFileController : Controller
    {
        public PhysicalFileController(ControllerName name, string? baseUri = default) : base(name, baseUri) { }

        [ResourceGet]
        public Task<Response> GetFileAsync(FileRequest request)
        {
            var path = CreatePath(request.ResourceName);

            return
                File.Exists(path)
                    ? OK<FileResponse>(File.OpenRead(path)).ToTask<Response>()
                    : NotFound<FileResponse>().ToTask<Response>();
        }

        [ResourcePut]
        public async Task<Response> CreateFileAsync(FileRequest request)
        {
            var path = CreatePath(request.ResourceName);

            using var fileStream = new FileStream(path, FileMode.CreateNew, FileAccess.Write);
            using var body = await request.CreateBodyStreamAsync();
            await body.Rewind().CopyToAsync(fileStream);
            await fileStream.FlushAsync();

            return OK<FileResponse>();
        }

        [ResourceDelete]
        public Task<Response> DeleteFileAsync(FileRequest request)
        {
            var path = CreatePath(request.ResourceName);
            File.Delete(path);
            return OK<FileResponse>().ToTask<Response>();
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