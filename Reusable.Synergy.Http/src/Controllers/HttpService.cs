using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Synergy.Data;
using Reusable.Synergy.Requests;

namespace Reusable.Synergy.Controllers;

[PublicAPI]
public abstract class HttpService : Service
{
    public HttpClient Client { get; set; } = new();

    public Action<HttpRequestHeaders> HeadersConfiguration { get; set; } = _ => { };

    public class Get : HttpService
    {
        public override async Task<object> InvokeAsync(IRequest request) => await InvokeAsync(HttpMethod.Get, ThrowIfNot<IHttpRequest>(request));
    }

    public class Post : HttpService
    {
        public override async Task<object> InvokeAsync(IRequest request) => await InvokeAsync(HttpMethod.Post, ThrowIfNot<IHttpRequest>(request));
    }

    public class Put : HttpService
    {
        public override async Task<object> InvokeAsync(IRequest request) => await InvokeAsync(HttpMethod.Put, ThrowIfNot<IHttpRequest>(request));
    }

    public class Delete : HttpService
    {
        public override async Task<object> InvokeAsync(IRequest request) => await InvokeAsync(HttpMethod.Delete, ThrowIfNot<IHttpRequest>(request));
    }

    private async Task<object> InvokeAsync(HttpMethod method, IHttpRequest request)
    {
        using var requestMessage = new HttpRequestMessage(method, request.Uri);

        if (request.Body is { } content)
        {
            requestMessage.Content = content as HttpContent ?? throw new Exception(); //new StreamContent(content.Rewind()));
            requestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(request.ContentType);
        }

        request.HeadersConfiguration(requestMessage.Headers);

        using var responseMessage = await Client.SendAsync(requestMessage, HttpCompletionOption.ResponseContentRead, request.CancellationToken).ConfigureAwait(false);
        
        return new HttpResponse
        {
            StatusCode = responseMessage.StatusCode,
            ContentType = responseMessage.Content.Headers.ContentType?.MediaType,
            Body = await CopyContentAsync(requestMessage)
        };
    }

    private static async Task<MemoryStream?> CopyContentAsync(HttpRequestMessage response)
    {
        var result = new MemoryStream();

        if (response.Content?.Headers.ContentLength > 0)
        {
            try
            {
                await response.Content.CopyToAsync(result);
                return result;
            }
            catch (Exception)
            {
                await result.DisposeAsync();
                throw;
            }
        }

        return default;
    }

    public override void Dispose() => Client.Dispose();
}