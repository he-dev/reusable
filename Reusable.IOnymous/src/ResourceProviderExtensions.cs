using System;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Exceptionize;
using Reusable.Quickey;

namespace Reusable.IOnymous
{
    public static class ResourceProviderExtensions
    {
        [Pure]
        [NotNull]
        public static TDecorator DecorateWith<TDecorable, TDecorator>(this TDecorable decorable, Func<TDecorable, TDecorator> decoratorFactory)
            where TDecorable : IResourceProvider
            where TDecorator : IResourceProvider
        {
            return decoratorFactory(decorable);
        }

        #region GET helpers

        // todo - move to config
        public static async Task<T> ReadObjectAsync<T>(this IResourceProvider resources, Request request)
        {
            using (var item = await resources.InvokeAsync(request))
            {
                using (var memoryStream = new MemoryStream())
                {
                    await item.CopyToAsync(memoryStream);

                    if (item.Format == MimeType.Text)
                    {
                        return (T)(object)await ResourceHelper.DeserializeTextAsync(memoryStream.Rewind());
                    }

                    if (item.Format == MimeType.Binary)
                    {
                        return (T)await ResourceHelper.DeserializeBinaryAsync<object>(memoryStream.Rewind());
                    }
                }

                throw DynamicException.Create
                (
                    $"ItemFormat",
                    $"Item's '{request.Uri}' format is '{item.Format}' but only '{MimeType.Binary}' and '{MimeType.Text}' are supported."
                );
            }
        }

        public static T ReadObject<T>(this IResourceProvider resources, Request request)
        {
            return resources.ReadObjectAsync<T>(request).GetAwaiter().GetResult();
        }

        #endregion

        #region PUT helpers

        #endregion

        #region POST helpers        

        public static async Task<IResource> PostAsync
        (
            this IResourceProvider provider,
            UriString uri,
            Func<Task<Stream>> serializeAsync,
            IImmutableSession properties = default
        )
        {
            return await provider.InvokeAsync(uri, serializeAsync, properties);
        }

        public static async Task<IResource> PutAsync
        (
            this IResourceProvider provider,
            UriString uri,
            Func<Task<Stream>> serializeAsync,
            IImmutableSession metadata = default
        )
        {
            return await provider.InvokeAsync(uri, serializeAsync, metadata);
        }

        private static async Task<IResource> InvokeAsync
        (
            this IResourceProvider provider,
            UriString uri,
            Func<Task<Stream>> serializeAsync,
            IImmutableSession properties
        )
        {
            var stream = await serializeAsync();
            var post = provider.InvokeAsync(new Request
            {
                Uri = uri,
                Method = RequestMethod.Post,
                Properties = properties,
                Body = stream
            });
            await post.ContinueWith(t =>
            {
                stream.Dispose();
                if (t.IsFaulted && !(t.Exception is null))
                {
                    throw t.Exception;
                }
            });

            return await post;
        }

        #endregion

        #region DELETE helpers

        #endregion

        private static readonly Action<Request> Pass = _ => { };

        public static async Task<IResource> GetAsync(this IResourceProvider resources, UriString uri, Action<Request> configure = default)
        {
            var request = new Request
            {
                Method = RequestMethod.Get,
                Uri = uri
            };
            (configure ?? Pass)(request);
            return await resources.InvokeAsync(request);
        }
        
        public static async Task<IResource> GetAsync(this IResourceProvider resources, UriString uri, IImmutableSession properties = default)
        {
            var request = new Request
            {
                Uri = uri,
                Method = RequestMethod.Get,
                Properties = properties.ThisOrEmpty()
            };
            return await resources.InvokeAsync(request);
        }
        
        public static async Task<IResource> PutAsync(this IResourceProvider resources, UriString uri, Stream body, IImmutableSession properties = default)
        {
            var request = new Request
            {
                Uri = uri,
                Method = RequestMethod.Put,
                Properties = properties.ThisOrEmpty(),
                Body = body
            };
            return await resources.InvokeAsync(request);
        }
    }
}