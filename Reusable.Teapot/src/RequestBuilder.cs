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
        IRequestBuilder Add(Action<RequestCopy> assert, bool canShortCircuit);

        void Assert(RequestCopy requestCopy);
    }

    // Maintains a collection of request-asserts.
    internal class RequestBuilder : IRequestBuilder
    {
        private readonly IList<(Action<RequestCopy> Assert, bool CanShortCircuit)> _asserts = new List<(Action<RequestCopy>, bool CanShortCircuit)>();
        
        // Adds a request assert. If it can-short-circuit then it can validate requests as they arrive.
        public IRequestBuilder Add(Action<RequestCopy> assert, bool canShortCircuit)
        {
            _asserts.Add((assert, canShortCircuit));
            return this;
        }

        // Fires all asserts.
        public void Assert(RequestCopy requestCopy)
        {
            foreach (var (assert, canShortCircuit) in _asserts)
            {
                if (requestCopy is null && !canShortCircuit)
                {
                    continue;
                }

                assert(requestCopy);
            }
        }
    }
}