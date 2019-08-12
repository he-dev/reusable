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
        private readonly UriString _baseUri;

        public PhysicalFileProvider(IImmutableContainer properties = default) : base(properties.ThisOrEmpty().SetScheme("file"))
        {
            Methods =
                MethodCollection
                    .Empty
                    .Add(RequestMethod.Get, GetAsync)
                    .Add(RequestMethod.Put, PutAsync)
                    .Add(RequestMethod.Delete, DeleteAsync);
        }

        public PhysicalFileProvider(string basePath, IImmutableContainer properties = default) : base(properties.ThisOrEmpty().SetScheme("file"))
        {
            _baseUri = basePath;
        }

        [ResourceGet]
        public Task<IResource> GetFileAsync(Request request)
        {
            return new PhysicalFile(request.Context.Copy(ResourceProperty.Selectors).SetUri(CreateUri(request.Uri))).ToTask<IResource>();
        }

        private Task<IResource> GetAsync(Request request)
        {
            return Task.FromResult<IResource>(new PhysicalFile(request.Context.Copy(ResourceProperty.Selectors).SetUri(request.Uri)));
        }

        private async Task<IResource> PutAsync(Request request)
        {
            using (var fileStream = new FileStream(request.Uri.ToUnc(), FileMode.CreateNew, FileAccess.Write))
            using (var body = await request.CreateBodyStreamAsync())
            {
                await body.Rewind().CopyToAsync(fileStream);
                await fileStream.FlushAsync();
            }

            return await GetAsync(request);
        }

        private Task<IResource> DeleteAsync(Request request)
        {
            File.Delete(request.Uri.ToUnc());
            return Task.FromResult<IResource>(new PhysicalFile(request.Context.Copy(ResourceProperty.Selectors).SetUri(request.Uri)));
        }

        private UriString CreateUri(UriString uri)
        {
            return
                _baseUri is null
                    ? uri
                    : Path.IsPathRooted(uri.Path.Decoded.ToString())
                        ? uri
                        : _baseUri + uri.Path.Decoded.ToString();
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