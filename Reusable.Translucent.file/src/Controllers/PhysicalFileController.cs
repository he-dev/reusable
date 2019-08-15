using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Extensions;
using Reusable.Quickey;

namespace Reusable.Translucent.Controllers
{
    [PublicAPI]
    public class PhysicalFileController : ResourceController
    {
        public PhysicalFileController()
            : this(ImmutableContainer.Empty
                .SetItem(SupportsRelativeUri, false)
            ) { }

        public PhysicalFileController(string basePath)
            : this(ImmutableContainer.Empty
                .SetItem(SupportsRelativeUri, true)
                .SetItem(BasePath, basePath)
            ) { }

        private PhysicalFileController(IImmutableContainer properties)
            : base(properties.UpdateItem(Schemes, s => s.Add(UriSchemes.Known.File))) { }

        [ResourceGet]
        public Task<Response> GetFileAsync(Request request)
        {
            var path = CreatePath(request.Uri);

            return
                File.Exists(path)
                    ? OK(File.OpenRead(path)).ToTask()
                    : NotFound().ToTask();
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

            return new Response.OK();
        }

        [ResourceDelete]
        public Task<Response> DeleteFileAsync(Request request)
        {
            var path = CreatePath(request.Uri);
            File.Delete(path);
            return OK().ToTask();
        }

        private string CreatePath(UriString uri)
        {
            var path = uri.ToUnc();

            return
                Path.IsPathRooted(path)
                    ? path
                    : Properties.TryGetItem(BasePath, out var basePath)
                        ? Path.Combine(basePath, path)
                        : path;
        }

        #region Properties

        private static readonly From<PhysicalFileController> This;

        public static Selector<string> BasePath { get; } = This.Select(() => BasePath);

        #endregion
    }

    //[UseType, UseMember]
    //[PlainSelectorFormatter]
    //public class PhysicalFileControllerProperties : SelectorBuilder<PhysicalFileControllerProperties> { }
}