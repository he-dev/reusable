using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Reusable.Exceptionize;
using Reusable.IOnymous;

namespace Reusable.Teapot
{
    public interface IResponseBuilder
    {
        [NotNull]
        HttpMethod Method { get; }

        [NotNull]
        UriString Uri { get; }

        [NotNull]
        ResponseBuilder Enqueue(Func<HttpRequest, ResponseMock> next);

        ResponseBuilder Clear();

        [NotNull]
        ResponseMock Next(HttpRequest request);
    }

    public class ResponseBuilder : IResponseBuilder
    {
        private readonly Queue<Func<HttpRequest, ResponseMock>> _responses = new Queue<Func<HttpRequest, ResponseMock>>();

        public ResponseBuilder(HttpMethod method, UriString uri)
        {
            Uri = uri;
            Method = method;
        }

        public UriString Uri { get; }

        public HttpMethod Method { get; }

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

        public ResponseMock Next(HttpRequest request)
        {
            while (_responses.Any())
            {
                var next = _responses.Peek();
                var response = next(request);
                if (response is null)
                {
                    _responses.Dequeue();
                }
                else
                {
                    return response;
                }
            }

            throw DynamicException.Create("OutOfResponses", "There are not more responses");
        }
    }
}