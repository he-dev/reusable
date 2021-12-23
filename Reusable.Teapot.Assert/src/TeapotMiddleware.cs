using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Reusable.Essentials;
using Reusable.Essentials.Extensions;
using HttpContext = Microsoft.AspNetCore.Http.HttpContext;
using HttpRequest = Microsoft.AspNetCore.Http.HttpRequest;
using RequestDelegate = Microsoft.AspNetCore.Http.RequestDelegate;

namespace Reusable.Teapot
{
    /// <summary>
    /// Validates requests as they arrive.
    /// </summary>
    public delegate void RequestAssertDelegate(RequestCopy requestCopy);

    /// <summary>
    /// Provides responses for each request.
    /// </summary>
    public delegate Func<HttpRequest, ResponseMock>? ResponseMockDelegate(HttpMethod method, string path);

    [UsedImplicitly]
    internal class TeapotMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly RequestAssertDelegate _requestAssert;
        private readonly ResponseMockDelegate _nextResponseMock;

        public TeapotMiddleware
        (
            RequestDelegate next,
            RequestAssertDelegate requestAssert,
            ResponseMockDelegate nextResponseMock
        )
        {
            _next = next;
            _requestAssert = requestAssert;
            _nextResponseMock = nextResponseMock;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                var uri = context.Request.Path + context.Request.QueryString;

                // You need this later to restore the body so don't dispose it.
                var bodyBackup = new MemoryStream();

                // You copy it because the original stream does not support seeking.
                await context.Request.Body.CopyToAsync(bodyBackup);

                // You copy the request because otherwise it won't pass the "barrier" between the middleware and the assert.
                using (var bodyCopy = new MemoryStream())
                {
                    await bodyBackup.Rewind().CopyToAsync(bodyCopy);

                    var request = new RequestCopy
                    {
                        Uri = uri,
                        Method = new HttpMethod(context.Request.Method),
                        // There is no copy-constructor.
                        Headers = new HeaderDictionary(context.Request.Headers.ToDictionary(x => x.Key, x => x.Value)),
                        ContentLength = context.Request.ContentLength,
                        Content = bodyCopy
                    };

                    _requestAssert(request);
                }

                // Restore body.
                context.Request.Body = bodyBackup;

                await _next(context);

                if (_nextResponseMock(new HttpMethod(context.Request.Method), uri) is {} responseMock)
                {
                    using var response = responseMock(context.Request);
                    
                    context.Response.StatusCode = response.StatusCode;
                    context.Response.ContentType = response.ContentType;

                    // Let's see what kind of content we got and handle it appropriately...

                    if (response.ContentType == MimeType.Plain)
                    {
                        await context.Response.WriteAsync((string)response.Content);
                    }

                    if (response.ContentType == MimeType.Binary)
                    {
                        await ((Stream)response.Content).Rewind().CopyToAsync(context.Response.Body);
                    }

                    if (response.ContentType == MimeType.Json)
                    {
                        await context.Response.WriteAsync(JsonConvert.SerializeObject(response.Content));
                    }
                }
                else
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                }
            }
            catch (Exception ex)
            {
                // It looks like the client sent an invalid request.
                if (ex is DynamicException dex && dex.NameStartsWith("Assert"))
                {
                    // "Response status code does not indicate success: 418 (I'm a teapot)."
                    context.Response.StatusCode = StatusCodes.Status418ImATeapot; // <-- I find this one funny. 
                }
                // Nope, there is a problem with the server.
                else
                {
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                }

                context.Response.ContentType = MimeType.Plain;
                await context.Response.WriteAsync(ex.ToString()); // <-- dump the exception to the response stream.
            }
        }
    }
}