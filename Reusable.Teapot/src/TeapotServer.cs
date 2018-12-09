using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Reusable.Reflection;
using Reusable.Teapot.Internal;

namespace Reusable.Teapot
{
    public class TeapotServer : IDisposable
    {
        private readonly ConcurrentDictionary<PathString, List<RequestInfo>> _requests;

        private readonly IWebHost _host;

        public TeapotServer(string url)
        {
            _requests = new ConcurrentDictionary<PathString, List<RequestInfo>>();

            _host =
                WebHost
                    .CreateDefaultBuilder()
                    .UseUrls(url)
                    .UseRequests(_requests)
                    .UseStartup<TeapotStartup>()
                    .Build();

            Task = Task.Run(async () =>
            {
                await _host.RunAsync();
            });
        }

        public Task Task { get; set; }

        [NotNull, ItemNotNull]
        public IEnumerable<RequestInfo> this[PathString path]
        {
            get
            {
                // ReSharper disable once ArrangeAccessorOwnerBody - I like it that way.
                return
                    _requests.TryGetValue(path, out var request)
                        ? request
                        : throw DynamicException.Create("RequestNotFound", $"There is no such request as '{path}'");
            }
        }

        public void Dispose()
        {
            _host.Dispose();
            foreach (var request in _requests.SelectMany(r => r.Value))
            {
                request.BodyStreamCopy?.Dispose();
            }
        }
    }
}
