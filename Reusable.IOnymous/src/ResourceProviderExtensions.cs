using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Exceptionize;
using Reusable.Quickey;

namespace Reusable.IOnymous
{
    public static class ResourceProviderExtensions
    {
//        [Pure]
//        [NotNull]
//        public static TDecorator DecorateWith<TDecorable, TDecorator>(this TDecorable decorable, Func<TDecorable, TDecorator> decoratorFactory)
//            where TDecorable : IResourceProvider
//            where TDecorator : IResourceProvider
//        {
//            return decoratorFactory(decorable);
//        }

        #region GET helpers

        //        // todo - move to config
        //        public static async Task<T> ReadObjectAsync<T>(this IResourceProvider resources, Request request)
        //        {
        //            using (var item = await resources.InvokeAsync(request))
        //            {
        //                using (var memoryStream = new MemoryStream())
        //                {
        //                    await item.CopyToAsync(memoryStream);
        //
        //                    if (item.Format == MimeType.Text)
        //                    {
        //                        return (T)(object)await ResourceHelper.DeserializeTextAsync(memoryStream.Rewind());
        //                    }
        //
        //                    if (item.Format == MimeType.Binary)
        //                    {
        //                        return (T)await ResourceHelper.DeserializeBinaryAsync<object>(memoryStream.Rewind());
        //                    }
        //                }
        //
        //                throw DynamicException.Create
        //                (
        //                    $"ItemFormat",
        //                    $"Item's '{request.Uri}' format is '{item.Format}' but only '{MimeType.Binary}' and '{MimeType.Text}' are supported."
        //                );
        //            }
        //        }

        //        public static T ReadObject<T>(this IResourceProvider resources, Request request)
        //        {
        //            return resources.ReadObjectAsync<T>(request).GetAwaiter().GetResult();
        //        }

        #endregion

        #region PUT helpers

        #endregion

        #region POST helpers        

        //        public static async Task<IResource> PostAsync
        //        (
        //            this IResourceProvider provider,
        //            UriString uri,
        //            Func<Task<Stream>> serializeBodyAsync,
        //            IImmutableContainer properties = default
        //        )
        //        {
        //            //return await provider.InvokeAsync(uri, serializeBodyAsync, properties);
        //            return await provider.InvokeAsync(new Request.Post(uri).SetProperties(_ => properties).SetBody(await serializeBodyAsync()));
        //        }

        //        public static async Task<IResource> PutAsync
        //        (
        //            this IResourceProvider provider,
        //            UriString uri,
        //            Func<Task<Stream>> serializeAsync,
        //            IImmutableContainer properties = default
        //        )
        //        {
        //            return await provider.InvokeAsync(uri, serializeAsync, properties);
        //        }

        //        private static async Task<IResource> InvokeAsync
        //        (
        //            this IResourceProvider provider,
        //            UriString uri,
        //            Func<Task<Stream>> serializeAsync,
        //            IImmutableContainer properties
        //        )
        //        {
        //            var stream = await serializeAsync();
        //            var invokeTask = provider.InvokeAsync(new Request
        //            {
        //                Uri = uri,
        //                Method = RequestMethod.Post,
        //                Properties = properties,
        //                Body = stream
        //            });
        //            await invokeTask.ContinueWith(t =>
        //            {
        //                stream.Dispose();
        //                if (t.IsFaulted && !(t.Exception is null))
        //                {
        //                    throw t.Exception;
        //                }
        //            });
        //
        //            return await invokeTask;
        //        }

        #endregion

        #region DELETE helpers

        #endregion

        //private static readonly Action<Request> Pass = _ => { };

        // public static async Task<IResource> GetAsync(this IResourceProvider resources, UriString uri, Action<Request> configure = default)
        // {
        //     var request = new Request.Get(uri);
        //     (configure ?? Pass)(request);
        //     return await resources.InvokeAsync(request);
        // }

        public static async Task<IResource> GetAsync(this IResourceProvider resources, UriString uri, IImmutableContainer properties = default)
        {
            return await resources.InvokeAsync(new Request.Get(uri)
            {
                Context = properties.ThisOrEmpty()
            });
        }

        public static IResourceProvider Use(this IResourceProvider provider, string name)
        {
            return new UseProvider(provider, name);
        }

        //        public static async Task<IResource> PutAsync(this IResourceProvider resources, UriString uri, Stream body, IImmutableContainer properties = default)
        //        {
        //            var request = new Request
        //            {
        //                Uri = uri,
        //                Method = RequestMethod.Put,
        //                Properties = properties.ThisOrEmpty(),
        //                Body = body
        //            };
        //            return await resources.InvokeAsync(request);
        //        }

        #region file:

        #region GET helpers

        //        public static async Task<IResource> GetFileAsync(this IResourceProvider provider, string path, MimeType format, IImmutableContainer context = default)
        //        {
        //            
        //        }

        public static async Task<IResource> GetFileAsync(this IResourceProvider resourceProvider, string path, MimeType format, IImmutableContainer properties = default)
        {
            return await resourceProvider.GetAsync(CreateUri(path), properties.ThisOrEmpty().SetItem(ResourceProperty.Format, format));
        }

        //        public static IResource GetTextFile(this IResourceProvider resourceProvider, string path, IImmutableSession metadata = default)
        //        {
        //            return resourceProvider.ReadFileAsync(path, MimeType.Text, metadata).GetAwaiter().GetResult();
        //        }

        public static async Task<string> ReadTextFileAsync(this IResourceProvider resourceProvider, string path, IImmutableContainer metadata = default)
        {
            using (var file = await resourceProvider.GetFileAsync(path, MimeType.Plain, metadata))
            {
                return await file.DeserializeTextAsync();
            }
        }

        public static string ReadTextFile(this IResourceProvider resourceProvider, string path, IImmutableContainer metadata = default)
        {
            using (var file = resourceProvider.GetFileAsync(path, MimeType.Plain, metadata).GetAwaiter().GetResult())
            {
                return file.DeserializeTextAsync().GetAwaiter().GetResult();
            }
        }

        #endregion

        #region PUT helpers

        public static async Task<IResource> WriteTextFileAsync(this IResourceProvider resourceProvider, string path, string value, IImmutableContainer properties = default)
        {
            return await resourceProvider.InvokeAsync(new Request.Put(CreateUri(path))
            {
                Body = value,
                CreateBodyStreamCallback = body => ResourceHelper.SerializeTextAsync((string)body, properties.ThisOrEmpty().GetItemOrDefault(ResourceProperty.Encoding, Encoding.UTF8)),
                Context = properties.ThisOrEmpty().SetItem(ResourceProperty.Format, MimeType.Plain)
            });
        }

        public static async Task<IResource> WriteFileAsync(this IResourceProvider resourceProvider, string path, CreateStreamCallback createStream, IImmutableContainer context = default)
        {
            return await resourceProvider.InvokeAsync(new Request.Put(CreateUri(path))
            {
                // Body must not be null.
                Body = Body.Null,
                CreateBodyStreamCallback = createStream,
                Context = context.ThisOrEmpty()
            });
        }

        #endregion

        #region POST helpers

        #endregion

        #region DELETE helpers

        public static async Task<IResource> DeleteFileAsync(this IResourceProvider resourceProvider, string path, IImmutableContainer metadata = default)
        {
            return await resourceProvider.InvokeAsync(new Request.Delete(CreateUri(path))
            {
                Context = metadata.ThisOrEmpty().SetItem(ResourceProperty.Format, MimeType.Plain)
            });
        }

        #endregion

        private static UriString CreateUri(string path)
        {
            return
                Path.IsPathRooted(path) || IsUnc(path)
                    ? new UriString("file", path)
                    : new UriString(path);
        }

        // https://www.pcmag.com/encyclopedia/term/53398/unc
        private static bool IsUnc(string value) => value.StartsWith("//");

        #endregion
    }
}