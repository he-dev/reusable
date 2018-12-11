using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Custom;
using System.Reactive;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Reusable.Reflection;
using Reusable.Teapot.Internal;

namespace Reusable.Teapot
{
    public class TeapotServer : IDisposable
    {
        private readonly IWebHost _host;

        private TeacupScope _teacup;

        public TeapotServer(string url)
        {
            _host =
                WebHost
                    .CreateDefaultBuilder()
                    .UseUrls(url)
                    .ConfigureServices(services =>
                    {
                        services.AddSingleton((LogDelegate)Log);
                        services.AddSingleton((ResponseDelegate)NextResponse);
                    })
                    .UseStartup<TeapotStartup>()
                    .Build();

            Task = _host.RunAsync();
        }

        public Task Task { get; set; }

        public ITeacupScope BeginScope() => (_teacup = new TeacupScope());

        private void Log(PathString path, SoftString method, RequestInfo request) => _teacup.Log(path, method, request);

        private Func<ResponseInfo> NextResponse(PathString path, SoftString method) => _teacup.NextResponse(path, method);

        public void Dispose()
        {
            _host.Dispose();
            _teacup?.Dispose();
        }
    }

    public interface ITeacupScope : IDisposable
    {
        IRequestAssert Requested(PathString path, SoftString method);

        void Responses(PathString path, string method, Action<ResponseQueueBuilder> responseQueueBuilder);
    }

    public class TeacupScope : ITeacupScope
    {
        private readonly RequestLog _requests = new RequestLog();

        private readonly ResponseQueue _responses = new ResponseQueue();

        public IRequestAssert Requested(PathString path, SoftString method)
        {
            return new RequestAssert
            (
                path: path,
                requests: _requests.TryGetValue((path, method), out var requests) && requests.Any()
                    ? requests
                    : throw DynamicException.Create("RequestNotFound", $"There was no '{method.ToString().ToUpper()}' requests to '{path.Value}'.")
            );
        }

        public void Responses(PathString path, string method, Action<ResponseQueueBuilder> responseQueueBuilder)
        {
            var builder = new ResponseQueueBuilder();
            responseQueueBuilder(builder);
            _responses.AddOrUpdate((path, method), builder.Build(), (k, q) => q);
        }

        public void Log(PathString path, SoftString method, RequestInfo request)
        {
            _requests.AddOrUpdate
            (
                (path, method),
                key => ImmutableList.Create(request),
                (key, log) => log.Add(request)
            );
        }

        public Func<ResponseInfo> NextResponse(PathString path, SoftString method)
        {
            if (_responses.TryGetValue((path, method), out var queue))
            {
                return
                    queue.Any()
                        ? queue.Dequeue()
                        : () => ResponseInfo.Empty;
            }

            return () => ResponseInfo.Empty;
        }

        public void Dispose()
        {
            foreach (var request in _requests.SelectMany(r => r.Value))
            {
                request.BodyStreamCopy?.Dispose();
            }
        }
    }

    public static class TeacupScopeExtensions
    {
        //public static IRequestAssert ClientRequested(this ITeacupScope teacup, PathString path, SoftString method)
        //{
        //    return new RequestAssert
        //    (
        //        path: path,
        //        requests: teacup.Requests(path, method) is var requests && requests.Any()
        //            ? requests
        //            : throw DynamicException.Create("RequestNotFound", $"There was no '{method.ToString().ToUpper()}' requests to '{path.Value}'.")
        //    );
        //}
    }

    public class ResponseQueueBuilder
    {
        private readonly Queue<Func<ResponseInfo>> _responses = new Queue<Func<ResponseInfo>>();

        public ResponseQueueBuilder Once(int statusCode, object content) => Exactly(statusCode, content, 1);

        public ResponseQueueBuilder Always(int statusCode, object content)
        {
            _responses.Enqueue(() => new ResponseInfo(statusCode, content));
            return this;
        }

        public ResponseQueueBuilder Exactly(int statusCode, object content, int count)
        {
            var counter = 0;
            _responses.Enqueue(() => counter++ < count ? new ResponseInfo(statusCode, content) : default);
            return this;
        }


        public Queue<Func<ResponseInfo>> Build() => _responses;
    }

    public readonly struct ResponseInfo
    {
        public ResponseInfo(int statusCode, object content)
        {
            StatusCode = statusCode;
            Content = content;
        }

        public static ResponseInfo Empty { get; } = default;

        public bool IsEmpty => StatusCode == 0 && Content == null;

        public int StatusCode { get; }

        public object Content { get; }
    }

    public interface IRequestAssert
    {
        PathString Path { get; }

        IImmutableList<RequestInfo> Requests { get; }
    }

    public interface IContentAssert<out TContent>
    {
        TContent Value { get; }
    }

    internal class RequestAssert : IRequestAssert
    {
        public RequestAssert(IImmutableList<RequestInfo> requests, PathString path)
        {
            Requests = requests;
            Path = path;
        }

        public PathString Path { get; }

        public IImmutableList<RequestInfo> Requests { get; }
    }

    internal class ContentAssert<TContent> : IContentAssert<TContent>
    {
        public ContentAssert(TContent value)
        {
            Value = value;
        }

        public TContent Value { get; }
    }

    public static class RequestAssertExtensions
    {
        public static IRequestAssert Times(this IRequestAssert assert, int times)
        {
            return
                assert.Requests.Count == times
                    ? assert
                    : throw DynamicException.Create(nameof(Times), $"Resource {assert.Path.Value} was requested {assert.Requests.Count} time(s) but expected {times}.");
        }

        public static IRequestAssert WithHeader(this IRequestAssert assert, string header, params string[] expectedValue)
        {
            var expectedHeaderValues = expectedValue.Join(", ");
            var requestsWithHeader = assert.Requests.Where(request => request.Headers.ContainsKey(header)).ToList();

            if (requestsWithHeader.Count < assert.Requests.Count)
            {
                throw DynamicException.Create
                (
                    Regex.Replace(header, @"\-", string.Empty),
                    $"{requestsWithHeader.Count} of {assert.Requests.Count} request(s) to '{assert.Path.Value}' did not specify the '{header}' header."
                );
            }

            if (expectedValue.Any())
            {
                // Some requests to ... specified other values [] than the expected [].
                var expected = requestsWithHeader.ToLookup(request => request.Headers[header].Equals(expectedValue));
                if (expected[false].Any())
                {
                    var invalidValues = expected[false].Select(request => request.Headers[header]).Join(", ");
                    throw DynamicException.Create
                    (
                        Regex.Replace(header, @"\-", string.Empty),
                        $"{expected[false].Count()} of {assert.Requests.Count} request(s) to '{assert.Path.Value}' specified the '{header}' header as [{invalidValues}] instead of '{expectedHeaderValues}'."
                    );
                }
            }

            return assert;
        }

        public static IRequestAssert WithApiVersion(this IRequestAssert assert, string version) => assert.WithHeader("Api-Version", version);

        public static IRequestAssert WithContentType(this IRequestAssert assert, string mediaType) => assert.WithHeader("Content-Type", mediaType);

        public static IRequestAssert WithContentTypeJson(this IRequestAssert assert, Action<IContentAssert<JToken>> contentAssert)
        {
            assert.WithHeader("Content-Type", "application/json; charset=utf-8");

            foreach (var request in assert.Requests)
            {
                var content = request.ToJToken();
                contentAssert(new ContentAssert<JToken>(content));
            }

            return assert;
        }

        public static IRequestAssert AsUserAgent(this IRequestAssert assert, string product, string version) => assert.WithHeader("User-Agent", $"{product}/{version}");

        public static IRequestAssert Accepts(this IRequestAssert assert, string mediaType) => assert.WithHeader("Accept", mediaType);

        public static IRequestAssert AcceptsJson(this IRequestAssert assert) => assert.Accepts("application/json");
    }

    public static class RequestBodyAssertExtensions
    {
        public static IContentAssert<JToken> HasProperty(this IContentAssert<JToken> content, string jsonPath)
        {
            return
                content.Value.SelectToken(jsonPath) != null
                    ? content
                    : throw DynamicException.Create("ContentPropertyNotFound", $"There is no such property as '{jsonPath}'");
        }
    }
}
