using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Reusable.Exceptionize;

namespace Reusable.Teapot
{
    public interface IResponseBuilder
    {
        [NotNull]
        ResponseBuilder Enqueue(Func<HttpRequest, ResponseMock> next);

        ResponseBuilder Clear();

        [NotNull]
        ResponseMock Next(HttpRequest request);
    }

    public class ResponseBuilder : IResponseBuilder
    {
        private readonly Queue<Func<HttpRequest, ResponseMock>> _responses = new Queue<Func<HttpRequest, ResponseMock>>();

        public ResponseBuilder Enqueue(Func<HttpRequest, ResponseMock> next)
        {
            _responses.Enqueue(next);
            return this;
        }

        public ResponseBuilder Clear()
        {
            _responses.Clear();
            return this;
        }

        // Gets the next response or throws when there are none.
        public ResponseMock Next(HttpRequest request)
        {
            while (_responses.Any())
            {
                var createResponse = _responses.Peek();
                var response = createResponse(request);

                // This response factory is empty.
                if (response is null)
                {
                    // Remove it from the queue an try again.
                    _responses.Dequeue();
                    continue;
                }

                return response;
            }

            throw DynamicException.Create("OutOfResponses", "There are no more responses");
        }
    }
}