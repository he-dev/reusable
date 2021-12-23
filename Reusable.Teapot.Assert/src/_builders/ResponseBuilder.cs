using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Reusable.Essentials;

namespace Reusable.Teapot
{
    public interface IResponseFactory
    {
        ResponseFactory Enqueue(Func<HttpRequest, ResponseMock> nextResponse);

        ResponseFactory Clear();

        ResponseMock Next(HttpRequest request);
    }

    public class ResponseFactory : IResponseFactory
    {
        private readonly Queue<Func<HttpRequest, ResponseMock?>> _responses = new Queue<Func<HttpRequest, ResponseMock?>>();

        public ResponseFactory Enqueue(Func<HttpRequest, ResponseMock?> nextResponse)
        {
            _responses.Enqueue(nextResponse);
            return this;
        }

        public ResponseFactory Clear()
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
                    // Remove it from the queue and try again.
                    _responses.Dequeue();
                }
                else
                {
                    return response;
                }
            }

            throw DynamicException.Create("OutOfResponses", $"There are no more responses for '{request.Method}/{request.Path}'.");
        }
    }
}