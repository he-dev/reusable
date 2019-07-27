using System;
using System.Collections.Generic;
using System.Net.Http;
using JetBrains.Annotations;
using Reusable.IOnymous;

namespace Reusable.Teapot
{
    public interface IRequestBuilder
    {
        [NotNull]
        HttpMethod Method { get; }

        [NotNull]
        UriString Uri { get; }

        [NotNull]
        IRequestBuilder Add(Action<TeacupRequest> assert, bool canShortCircuit);

        void Assert(TeacupRequest request);
    }

    internal class RequestBuilder : IRequestBuilder
    {
        private readonly IList<(Action<TeacupRequest> Assert, bool CanShortCircuit)> _asserts = new List<(Action<TeacupRequest>, bool CanShortCircuit)>();

        public RequestBuilder(HttpMethod method, UriString uri)
        {
            Uri = uri;
            Method = method;
        }

        public HttpMethod Method { get; }

        public UriString Uri { get; }

        public IRequestBuilder Add(Action<TeacupRequest> assert, bool canShortCircuit)
        {
            _asserts.Add((assert, canShortCircuit));
            return this;
        }

        public void Assert(TeacupRequest request)
        {
            foreach (var (assert, canShortCircuit) in _asserts)
            {
                if (request is null && !canShortCircuit)
                {
                    continue;
                }

                assert(request);
            }
        }
    }
}