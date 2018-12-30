using System;
using System.Collections.Concurrent;
using JetBrains.Annotations;
using Reusable.Teapot;

namespace Reusable.Tests.XUnit.Fixtures
{
    [UsedImplicitly]
    public class TeapotFactoryFixture : IDisposable
    {
        private readonly ConcurrentDictionary<string, TeapotServer> _servers;

        public TeapotFactoryFixture()
        {
            _servers = new ConcurrentDictionary<string, TeapotServer>();
        }

        public TeapotServer CreateTeapotServer(string url)
        {
            return _servers.GetOrAdd(url, u => new TeapotServer(u));
        }

        public void Dispose()
        {
            foreach (var teapotServer in _servers.Values)
            {
                teapotServer.Dispose();
            }
        }
    }
}