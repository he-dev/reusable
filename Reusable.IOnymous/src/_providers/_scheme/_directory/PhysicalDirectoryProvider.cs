using System;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Quickey;

namespace Reusable.IOnymous
{
    [PublicAPI]
    public class PhysicalDirectoryProvider : ResourceProvider
    {
        public PhysicalDirectoryProvider(IImmutableContainer properties = default)
            : base(properties.ThisOrEmpty().SetScheme("directory"))
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
            return Task.FromResult<IResource>(
                new PhysicalDirectory(
                    ImmutableContainer
                        .Empty
                        .SetItem(Resource.Property.Uri, request.Uri)));
        }

        private async Task<IResource> PutAsync(Request request)
        {
            //using (var streamReader = new StreamReader(request.Body))
            {
                var fullName = request.Uri.Path.Decoded.ToString(); //, await streamReader.ReadToEndAsync());
                Directory.CreateDirectory(fullName);
                return await GetAsync(new Request { Uri = fullName });
            }
        }

        private async Task<IResource> DeleteAsync(Request request)
        {
            Directory.Delete(request.Uri.Path.Decoded.ToString(), true);
            return await GetAsync(request);
        }
    }

    [PublicAPI]
    internal class PhysicalDirectory : Resource
    {
        public PhysicalDirectory(IImmutableContainer properties)
            : base(properties
                .SetItem(Property.Exists, Directory.Exists(properties.GetItemOrDefault(Property.Uri).Path.Decoded.ToString()))
                .SetItem(Property.Format, MimeType.None)
                .SetItem(Property.ModifiedOn, p =>
                {
                    return
                        p.GetItemOrDefault(Property.Exists)
                            ? Directory.GetLastWriteTimeUtc(properties.GetItemOrDefault(Property.Uri).Path.Decoded.ToString())
                            : DateTime.MinValue;
                })) { }

        public override Task CopyToAsync(Stream stream) => throw new NotSupportedException();
    }
}