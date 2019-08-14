using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Extensions;
using Reusable.Quickey;

namespace Reusable.IOnymous.Controllers
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
        public Task<IResource> GetFileAsync(Request request)
        {
            return new PhysicalFile(request.Metadata.Copy(ResourceProperties.Selectors).SetItem(ResourceProperties.Uri, CreateUri(request.Uri))).ToTask<IResource>();
        }

        [ResourcePut]
        public async Task<IResource> CreateFileAsync(Request request)
        {
            using (var fileStream = new FileStream(request.Uri.ToUnc(), FileMode.CreateNew, FileAccess.Write))
            using (var body = await request.CreateBodyStreamAsync())
            {
                await body.Rewind().CopyToAsync(fileStream);
                await fileStream.FlushAsync();
            }

            return await GetFileAsync(request);
        }

        [ResourceDelete]
        public Task<IResource> DeleteFileAsync(Request request)
        {
            File.Delete(request.Uri.ToUnc());
            return new PhysicalFile(request.Metadata.Copy(ResourceProperties.Selectors).SetItem(ResourceProperties.Uri, request.Uri)).ToTask<IResource>();
        }

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

    [PublicAPI]
    internal class PhysicalFile : Resource
    {
        public PhysicalFile(IImmutableContainer properties)
            : base(properties
                    .SetItem(ResourceProperties.Exists, p => File.Exists(p.GetItemOrDefault(ResourceProperties.Uri).ToUnc()))
                    .SetItem(ResourceProperties.Length, p =>
                        p.GetItem(ResourceProperties.Exists)
                            ? new FileInfo(p.GetItemOrDefault(ResourceProperties.Uri).ToUnc()).Length
                            : -1)
                //.SetItem(PropertySelector.Select(x => x.ModifiedOn), p => )
            ) { }

        //public override bool Exists => File.Exists(Uri.ToUnc());

        //public override long? Length => new FileInfo(Uri.ToUnc()).Length;

        //public override DateTime? CreatedOn => Exists ? File.GetCreationTimeUtc(Uri.ToUnc()) : default;

        //public override DateTime? ModifiedOn => Exists ? File.GetLastWriteTimeUtc(Uri.ToUnc()) : default;

        public override async Task CopyToAsync(Stream stream)
        {
            using (var fileStream = File.OpenRead(Uri.ToUnc()))
            {
                await fileStream.CopyToAsync(stream);
            }
        }
    }
}