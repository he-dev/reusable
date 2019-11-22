﻿using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.Translucent.Annotations;

namespace Reusable.Translucent.Controllers
{
    [PublicAPI]
    [Handles(typeof(FileRequest))]
    public class PhysicalFileController : ResourceController
    {
        public PhysicalFileController(string? id, string? basePath = default) : base(id, basePath, UriSchemes.Known.File)
        {
            BasePath = basePath;
        }

        public string BasePath { get; }
        
        [ResourceGet]
        public Task<Response> GetFileAsync(Request request)
        {
            var path = CreatePath(request.Uri);

            return
                File.Exists(path)
                    ? OK<FileResponse>(File.OpenRead(path)).ToTask()
                    : NotFound<FileResponse>().ToTask();
        }

        [ResourcePut]
        public async Task<Response> CreateFileAsync(Request request)
        {
            var path = CreatePath(request.Uri);

            using (var fileStream = new FileStream(path, FileMode.CreateNew, FileAccess.Write))
            using (var body = await request.CreateBodyStreamAsync())
            {
                await body.Rewind().CopyToAsync(fileStream);
                await fileStream.FlushAsync();
            }

            return OK<FileResponse>();
        }

        [ResourceDelete]
        public Task<Response> DeleteFileAsync(Request request)
        {
            var path = CreatePath(request.Uri);
            File.Delete(path);
            return OK<FileResponse>().ToTask();
        }

        private string CreatePath(UriString uri)
        {
            var path = uri.ToUnc();

            return
                Path.IsPathRooted(path)
                    ? path
                    : BasePath is {} basePath
                        ? Path.Combine(basePath, path)
                        : path;
        }
    }

    //[UseType, UseMember]
    //[PlainSelectorFormatter]
    //public class PhysicalFileControllerProperties : SelectorBuilder<PhysicalFileControllerProperties> { }
}