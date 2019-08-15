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
        public PhysicalFileController(IImmutableContainer properties = default)
            : base(properties.ThisOrEmpty()
                .UpdateItem(ResourceControllerProperties.Schemes, s => s.Add(UriSchemes.Known.File))
                .SetItem(ResourceControllerProperties.SupportsRelativeUri, true)
            ) { }

        [ResourceGet]
        public Task<Response> GetFileAsync(Request request)
        {
            var path = CreateUri(request.Uri).Path.Decoded.ToString();

            return
                System.IO.File.Exists(path)
                    ? new Response.OK { Body = System.IO.File.OpenRead(path) }.ToTask<Response>()
                    : new Response.NotFound().ToTask<Response>();
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

            return await GetFileAsync(request);
        }

        // [ResourceDelete]
        // public Task<Response> DeleteFileAsync(Request request)
        // {
        //     System.IO.File.Delete(request.Uri.ToUnc());
        //     return new PhysicalFile(request.Metadata.Copy(ResourceProperties.Selectors).SetItem(ResourceProperties.Uri, request.Uri)).ToTask<IResource>();
        // }

        private UriString CreateUri(UriString uri)
        {
            return
                Path.IsPathRooted(uri.Path.Decoded.ToString())
                    ? uri
                    : Properties.TryGetItem(PhysicalFileControllerProperties.BaseUri, out var baseUri)
                        ? baseUri + uri.Path.Decoded.ToString()
                        : uri;
        }
    }

    [UseType, UseMember]
    [PlainSelectorFormatter]
    public class PhysicalFileControllerProperties : SelectorBuilder<PhysicalFileControllerProperties>
    {
        public static Selector<UriString> BaseUri { get; } = Select(() => BaseUri);
    }
}