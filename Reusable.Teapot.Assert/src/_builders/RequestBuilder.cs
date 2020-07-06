using System;
using System.Collections.Generic;

namespace Reusable.Teapot
{
    public interface IRequestAssert
    {
        IRequestAssert Add(Action<RequestCopy?> assert);

        void Assert(RequestCopy? requestCopy);
    }

    // Maintains a collection of request-asserts.
    internal class RequestAssert : IRequestAssert
    {
        private readonly IList<Action<RequestCopy>> _asserts = new List<Action<RequestCopy>>();
        
        // Adds a request assert. If it can-short-circuit then it can validate requests as they arrive.
        public IRequestAssert Add(Action<RequestCopy> assert)
        {
            _asserts.Add(assert);
            return this;
        }

        // Fires all asserts.
        public void Assert(RequestCopy? requestCopy)
        {
            foreach (var assert in _asserts)
            {
                assert(requestCopy);
            }
        }
    }
}