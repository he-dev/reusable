using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Linq.Custom;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Extensions;
using Reusable.Quickey;

namespace Reusable.IOnymous
{
    [PublicAPI]
    public class PhysicalFileProvider : ResourceProvider
    {
        public PhysicalFileProvider(IImmutableContainer properties = default) 
            : base(properties.ThisOrEmpty()
                .SetScheme(UriSchemes.Known.File)
                .SetItem(ResourceProviderProperty.SupportsRelativeUri, true)
            ) 
        { }

        [ResourceGet]
        public Task<IResource> GetFileAsync(Request request)
        {
            return new PhysicalFile(request.Context.Copy(ResourceProperty.Selectors).SetUri(CreateUri(request.Uri))).ToTask<IResource>();
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
            return new PhysicalFile(request.Context.Copy(ResourceProperty.Selectors).SetUri(request.Uri)).ToTask<IResource>();
        }

        private UriString CreateUri(UriString uri)
        {
            return
                Path.IsPathRooted(uri.Path.Decoded.ToString())
                    ? uri
                    : Properties.TryGetItem(PropertySelectors.BaseUri, out var baseUri)
                        ? baseUri + uri.Path.Decoded.ToString()
                        : uri;
        }

        [UseType, UseMember]
        [PlainSelectorFormatter]
        public class PropertySelectors : SelectorBuilder<PropertySelectors>
        {
            public static Selector<UriString> BaseUri { get; } = Select(() => BaseUri);
        }
    }

    [PublicAPI]
    internal class PhysicalFile : Resource
    {
        public PhysicalFile(IImmutableContainer properties)
            : base(properties
                    .SetItem(ResourceProperty.Exists, p => File.Exists(p.GetItemOrDefault(ResourceProperty.Uri).ToUnc()))
                    .SetItem(ResourceProperty.Length, p =>
                        p.GetExists()
                            ? new FileInfo(p.GetItemOrDefault(ResourceProperty.Uri).ToUnc()).Length
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