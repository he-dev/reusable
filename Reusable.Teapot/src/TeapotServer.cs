using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Custom;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Reusable.Exceptionizer;
using Reusable.IOnymous;

namespace Reusable.Teapot
{
    [PublicAPI]
    public class TeapotServer : IDisposable
    {
        private readonly IWebHost _host;

        [CanBeNull]
        private TeacupScope _teacup;

        public TeapotServer(string url)
        {
            _host =
                WebHost
                    .CreateDefaultBuilder()
                    .UseUrls(url)
                    .ConfigureServices(services =>
                    {
                        services.AddSingleton((RequestAssertDelegate)Assert);
                        services.AddSingleton((ResponseDelegate)NextResponse);
                    })
                    .UseStartup<TeapotStartup>()
                    .Build();

            Task = _host.RunAsync();
        }

        public Task Task { get; set; }

        public ITeacupScope BeginScope() => (_teacup = new TeacupScope());

        private void Assert(RequestInfo request)
        {
            if (_teacup is null) throw new InvalidOperationException($"Cannot get response without scope. Call '{nameof(BeginScope)}' first.");
            
            _teacup.Assert(request);
        }

        private Func<HttpRequest, ResponseInfo> NextResponse(UriString path, SoftString method)
        {
            if (_teacup is null) throw new InvalidOperationException($"Cannot get response without scope. Call '{nameof(BeginScope)}' first.");
            
            return _teacup.NextResponse(path, method);
        }

        public void Dispose()
        {
            _host.Dispose();
            _teacup?.Dispose();
        }
    }

    public interface ITeacupScope : IDisposable
    {
        RequestMock Mock(string uri);
    }

    public class TeacupScope : ITeacupScope
    {
        private readonly IList<RequestMock> _mocks = new List<RequestMock>();

        public RequestMock Mock(string uri)
        {
            var mock = new RequestMock(uri);
            _mocks.Add(mock);
            return mock;
        }

        public void Assert(RequestInfo request)
        {
            foreach (var mock in _mocks.Where(m => m.Uri == request.Uri))
            {
                mock.Assert(request);
            }
        }

        public Func<HttpRequest, ResponseInfo> NextResponse(UriString uri, SoftString method)
        {
            var mock = _mocks.FirstOrDefault(m => m.Uri == uri);
            return mock?.NextResponse(method);
        }

        public void Dispose()
        {
            foreach (var mock in _mocks)
            {
                mock.Dispose();
            }
        }
    }

    public class RequestMock : IDisposable
    {
        public static readonly string AnyMethod = string.Empty;
        
        private readonly IList<(IRequestBuilder Request, IResponseBuilder Response)> _methods = new List<(IRequestBuilder Request, IResponseBuilder Response)>();

        public RequestMock(UriString uri) => Uri = uri;

        public UriString Uri { get; }

        public RequestMock Arrange(string method, Action<IRequestBuilder, IResponseBuilder> configure)
        {
            if (_methods.Any(m => m.Request.Method == method)) throw new ArgumentException($"Method {method.ToUpper()} has already been added.");

            var request = new RequestBuilder(Uri, method);
            var response = new ResponseBuilder(Uri, method);
            configure(request, response);
            _methods.Add((request, response));
            return this;
        }

        public void Assert(RequestInfo request)
        {
            foreach (var method in _methods.Where(m => m.Request.Method.In(AnyMethod, request.Method)))
            {
                method.Request.Assert(request);
            }
        }

        public void Assert()
        {
            foreach (var method in _methods)
            {
                method.Request.Assert(default);
            }
        }

        public Func<HttpRequest, ResponseInfo> NextResponse(SoftString method)
        {
            var response = _methods.SingleOrDefault(m => m.Response.Method == method).Response;
            return request => response?.Next(request);
        }

        public void Dispose()
        {
        }
    }

    public interface IResponseBuilder
    {
        [NotNull]
        UriString Uri { get; }

        [NotNull]
        SoftString Method { get; }

        [NotNull]
        ResponseBuilder Enqueue(Func<HttpRequest, ResponseInfo> next);

        [NotNull]
        ResponseInfo Next(HttpRequest request);
    }

    public class ResponseBuilder : IResponseBuilder
    {
        private readonly Queue<Func<HttpRequest, ResponseInfo>> _responses = new Queue<Func<HttpRequest, ResponseInfo>>();

        public ResponseBuilder(UriString uri, SoftString method)
        {
            Uri = uri;
            Method = method;
        }

        public UriString Uri { get; }

        public SoftString Method { get; }

        public ResponseBuilder Enqueue(Func<HttpRequest, ResponseInfo> next)
        {
            _responses.Enqueue(next);
            return this;
        }

        public ResponseInfo Next(HttpRequest request)
        {
            while (_responses.Any())
            {
                var next = _responses.Peek();
                var response = next(request);
                if (response is null)
                {
                    _responses.Dequeue();
                }
                else
                {
                    return response;
                }
            }

            throw DynamicException.Create("OutOfResponses", "There are not more responses");
        }
    }


    public class ResponseInfo : IDisposable
    {
        public ResponseInfo(int statusCode, object content)
        {
            StatusCode = statusCode;
            Content = content;
        }

        public int StatusCode { get; }

        [CanBeNull]
        public object Content { get; }

        public void Dispose()
        {
            (Content as IDisposable)?.Dispose();
        }
    }

    public interface IRequestBuilder
    {
        [NotNull]
        UriString Uri { get; }

        [NotNull]
        SoftString Method { get; }

        [NotNull]
        IRequestBuilder Add(Action<RequestInfo> assert, bool allowRequestNull);

        void Assert(RequestInfo request);
    }

    internal class RequestBuilder : IRequestBuilder
    {
        private readonly IList<(Action<RequestInfo> Assert, bool AllowRequestNull)> _asserts = new List<(Action<RequestInfo>, bool)>();

        public RequestBuilder(UriString uri, SoftString method)
        {
            Uri = uri;
            Method = method;
        }

        public UriString Uri { get; }

        public SoftString Method { get; }

        public IRequestBuilder Add(Action<RequestInfo> assert, bool allowRequestNull)
        {
            _asserts.Add((assert, allowRequestNull));
            return this;
        }

        public void Assert(RequestInfo request)
        {
            foreach (var (assert, allowRequestNull) in _asserts)
            {
                if (request is null && !allowRequestNull)
                {
                    continue;
                }

                assert(request);
            }
        }
    }

    public interface IContentAssert<out TContent>
    {
        [CanBeNull]
        TContent Value { get; }
    }

    internal class ContentAssert<TContent> : IContentAssert<TContent>
    {
        public ContentAssert(TContent value) => Value = value;

        public TContent Value { get; }
    }

    [PublicAPI]
    public static class RequestBuilderExtensions
    {
        public static IRequestBuilder Occurs(this IRequestBuilder builder, int exactly)
        {
            var counter = 0;
            return builder.Add(request =>
            {
                if (request is null)
                {
                    if (counter != exactly)
                    {
                        throw DynamicException.Create(nameof(Occurs), $"Resource {builder.Uri} was requested {counter} time(s) but expected {exactly}.");
                    }
                }
                else
                {
                    if (++counter > exactly)
                    {
                        throw DynamicException.Create(nameof(Occurs), $"Resource {builder.Uri} was requested {counter} time(s) but expected {exactly}.");
                    }
                }
            }, true);
        }

        public static IRequestBuilder WithHeader(this IRequestBuilder builder, string header, params string[] expectedValue)
        {
            //var expectedHeader = Regex.Replace(header, @"\-", string.Empty);

            return builder.Add(request =>
            {
                if (request.Headers.TryGetValue(header, out var values))
                {
                    if (values.Intersect(expectedValue).Count() != expectedValue.Count())
                    {
                        throw DynamicException.Create
                        (
                            "DifferentHeaderValue",
                            $"Header '{header}' has different values."
                        );
                    }
                }
                else
                {
                    throw DynamicException.Create
                    (
                        "HeaderNotFound",
                        $"Header '{header}' is missing."
                    );
                }
            }, false);
        }

        public static IRequestBuilder WithApiVersion(this IRequestBuilder builder, string version) => builder.WithHeader("Api-Version", version);

        public static IRequestBuilder WithContentType(this IRequestBuilder builder, string mediaType) => builder.WithHeader("Content-Type", mediaType);

        public static IRequestBuilder WithContentTypeJson(this IRequestBuilder builder, Action<IContentAssert<JToken>> contentAssert)
        {
            //assert.WithHeader("Content-Type", "application/json; charset=utf-8");

            return builder.Add(request =>
            {
                var content = request.DeserializeAsJToken();
                contentAssert(new ContentAssert<JToken>(content));
            }, false);
        }

        public static IRequestBuilder AsUserAgent(this IRequestBuilder builder, string product, string version) => builder.WithHeader("User-Agent", $"{product}/{version}");

        public static IRequestBuilder Accepts(this IRequestBuilder builder, string mediaType) => builder.WithHeader("Accept", mediaType);

        public static IRequestBuilder AcceptsJson(this IRequestBuilder builder) => builder.Accepts("application/json");
    }


    public static class RequestBodyAssertExtensions
    {
        public static IContentAssert<JToken> HasProperty(this IContentAssert<JToken> content, string jsonPath)
        {
            if (content.Value is null)
            {
                throw DynamicException.Create("ContentNull", "There is no content.");
            }

            return
                content.Value.SelectToken(jsonPath) is null
                    ? throw DynamicException.Create("ContentPropertyNotFound", $"There is no such property as '{jsonPath}'")
                    : content;
        }
    }

    public static class RequestMockExtensions
    {
        public static RequestMock ArrangeGet(this RequestMock requestMock, Action<IRequestBuilder, IResponseBuilder> configure)
        {
            return requestMock.Arrange("GET", configure);
        }

        public static RequestMock ArrangePost(this RequestMock requestMock, Action<IRequestBuilder, IResponseBuilder> configure)
        {
            return requestMock.Arrange("POST", configure);
        }

        public static RequestMock ArrangePut(this RequestMock requestMock, Action<IRequestBuilder, IResponseBuilder> configure)
        {
            return requestMock.Arrange("PUT", configure);
        }

        public static RequestMock ArrangeDelete(this RequestMock requestMock, Action<IRequestBuilder, IResponseBuilder> configure)
        {
            return requestMock.Arrange("DELETE", configure);
        }

        public static RequestMock ArrangeAny(this RequestMock requestMock, Action<IRequestBuilder, IResponseBuilder> configure)
        {
            return requestMock.Arrange(RequestMock.AnyMethod, configure);
        }
    }

    [PublicAPI]
    public static class ResponseBuilderExtensions
    {
        public static IResponseBuilder Once(this IResponseBuilder response, int statusCode, object content)
        {
            return response.Exactly(statusCode, content, 1);
        }

        public static IResponseBuilder Always(this IResponseBuilder response, int statusCode, object content)
        {
            return response.Enqueue(request => new ResponseInfo(statusCode, content));
        }

        public static IResponseBuilder Exactly(this IResponseBuilder response, int statusCode, object content, int count)
        {
            var counter = 0;
            return response.Enqueue(request => counter++ < count ? new ResponseInfo(statusCode, content) : default);
        }

        public static IResponseBuilder Echo(this IResponseBuilder response)
        {
            return response.Enqueue(request =>
            {
                var requestCopy = new MemoryStream();
                request.Body.Seek(0, SeekOrigin.Begin);
                request.Body.CopyTo(requestCopy);
                return new ResponseInfo(200, requestCopy);
            });
        }
    }
}