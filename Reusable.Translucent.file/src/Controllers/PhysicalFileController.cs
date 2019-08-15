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
            : base(ImmutableContainer.Empty
                .SetItem(SupportsRelativeUri, false)
            ) { }

        public PhysicalFileController(string basePath)
            : base(ImmutableContainer.Empty
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
                    ? OK(File.OpenRead(path), request.Metadata.GetItem(ResourceProperties.Accept)).ToTask()
                    : NotFound().ToTask();
        }

        [ResourcePut]
        public async Task<Response> CreateFileAsync(Request request)
        {
            using (var fileStream = new FileStream(request.Uri.ToUnc(), FileMode.CreateNew, FileAccess.Write))
            using (var body = await request.CreateBodyStreamAsync())
            {
                await body.Rewind().CopyToAsync(fileStream);
                await fileStream.FlushAsync();
            }

            return new Response.OK();
        }

        // [ResourceDelete]
        // public Task<Response> DeleteFileAsync(Request request)
        // {
        //     System.IO.File.Delete(request.Uri.ToUnc());
        //     return new PhysicalFile(request.Metadata.Copy(ResourceProperties.Selectors).SetItem(ResourceProperties.Uri, request.Uri)).ToTask<IResource>();
        // }

        private string CreatePath(UriString uri)
        {
            var path = uri.Path.Decoded.ToString();

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