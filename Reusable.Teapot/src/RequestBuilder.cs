using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Reusable.IOnymous;

namespace Reusable.Teapot
{
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
    
    
}