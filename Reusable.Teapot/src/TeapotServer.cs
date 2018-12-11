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
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;
using Reusable.Reflection;
using Reusable.Teapot.Internal;

namespace Reusable.Teapot
{
    public class TeapotServer : IDisposable
    {
        private readonly IWebHost _host;

        private readonly ISet<IObserver<RequestInfo>> _observers = new HashSet<IObserver<RequestInfo>>();

        public TeapotServer(string url)
        {
            _host =
                WebHost
                    .CreateDefaultBuilder()
                    .UseUrls(url)
                    //.UseRequests(_requests)
                    .ConfigureServices(services =>
                    {
                        services.AddSingleton((ObserversDelegate)(() => _observers));
                    })
                    .UseStartup<TeapotStartup>()
                    .Build();

            Task = _host.RunAsync();
        }

        public Task Task { get; set; }

        public TeacupScope BeginScope()
        {
            var teacup = default(TeacupScope);
            var unsubscribe = Disposable.Create(() => _observers.Remove(teacup));
            teacup = new TeacupScope(unsubscribe);
            _observers.Add(teacup);
            return teacup;
        }

        public void Dispose()
        {
            _host.Dispose();
        }
    }

    public class TeacupScope : IObserver<RequestInfo>, IDisposable
    {
        private readonly RequestLog _requests = new RequestLog();

        private readonly IDisposable _unsubscribe;

        private readonly IObserver<RequestInfo> _observer;

        public TeacupScope(IDisposable unsubscribe)
        {
            _unsubscribe = unsubscribe;
            _observer = Observer.Create<RequestInfo>
            (
                onNext: request =>
                {
                    _requests.AddOrUpdate
                    (
                        request.Path,
                        path => ImmutableList.Create(request),
                        (path, log) => log.Add(request)
                    );
                },
                onError: inner => { /* not sure what to do */}
            );
        }

        [NotNull, ItemNotNull]
        public IImmutableList<RequestInfo> this[PathString path] => _requests.TryGetValue(path, out var request) ? request : ImmutableList<RequestInfo>.Empty;

        #region IObserver<RequestInfo>

        public void OnNext(RequestInfo value) => _observer.OnNext(value);

        public void OnError(Exception error) => _observer.OnError(error);

        public void OnCompleted() => _observer.OnCompleted();

        #endregion

        public void Dispose()
        {
            _unsubscribe.Dispose();
            foreach (var request in _requests.SelectMany(r => r.Value))
            {
                request.BodyStreamCopy?.Dispose();
            }
        }
    }

    public static class TeacupScopeExtensions
    {
        public static IRequestAssert ClientRequested(this TeacupScope teacup, PathString path)
        {
            return new RequestAssert
            (
                path: path,
                requests: teacup[path] is var requests && requests.Any()
                    ? requests
                    : throw DynamicException.Create("RequestNotFound", $"There is no such request as '{path.Value}'")
            );
        }

        public static void WhenRequested(this TeacupScope teacup, PathString path, string method)
        {

        }
    }

    public class ResponseConfiguration
    {
        public PathString Path { get; set; }

        public string Method { get; set; }

        public IList<(string StatusCode, object Content)> Responses { get; set; }

        public ResponseConfiguration Add(string statusCode, object content)
        {
            return this;
        }
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
