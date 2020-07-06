using System;
using System.Collections.Concurrent;
using JetBrains.Annotations;
using Reusable.Teapot;

namespace Reusable.Utilities.XUnit.Fixtures
{
    [UsedImplicitly]
    public class TeapotServerFixture : IDisposable
    {
        private readonly ConcurrentDictionary<string, AssertServer> _servers;

        public TeapotServerFixture() => _servers = new ConcurrentDictionary<string, AssertServer>();

        public AssertServer GetServer(string url) => _servers.GetOrAdd(url, u => new AssertServer(u));

        public void Dispose()
        {
            foreach (var server in _servers.Values)
            {
                server.Dispose();
            }
        }
    }
}