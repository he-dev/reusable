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
        public PhysicalFileProvider(IImmutableSession properties = default) : base(properties.ThisOrEmpty().SetScheme("file"))
        {
            Methods =
                MethodDictionary
                    .Empty
                    .Add(RequestMethod.Get, GetAsync)
                    .Add(RequestMethod.Put, PutAsync)
                    .Add(RequestMethod.Delete, DeleteAsync);
        }

        private Task<IResource> GetAsync(Request request)
        {
            return Task.FromResult<IResource>(new PhysicalFile(request.Properties.Copy(Resource.PropertySelector).SetUri(request.Uri)));
        }

        private async Task<IResource> PutAsync(Request request)
        {
            using (var fileStream = new FileStream(request.Uri.ToUnc(), FileMode.CreateNew, FileAccess.Write))
            {
                await request.Body.Rewind().CopyToAsync(fileStream);
                await fileStream.FlushAsync();
            }

            return await GetAsync(request);
        }

        private Task<IResource> DeleteAsync(Request request)
        {
            File.Delete(request.Uri.ToUnc());
            return Task.FromResult<IResource>(new PhysicalFile(request.Properties.Copy(Resource.PropertySelector).SetUri(request.Uri)));
        }
    }

    [PublicAPI]
    internal class PhysicalFile : Resource
    {
        public PhysicalFile(IImmutableSession properties)
            : base(properties
                    .SetItem(PropertySelector.Select(x => x.Exists), p => File.Exists(p.GetItemOrDefault(PropertySelector.Select(x => x.Uri)).ToUnc()))
                    .SetItem(PropertySelector.Select(x => x.Length), p =>
                        p.GetExists()
                            ? new FileInfo(p.GetItemOrDefault(PropertySelector.Select(x => x.Uri)).ToUnc()).Length
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