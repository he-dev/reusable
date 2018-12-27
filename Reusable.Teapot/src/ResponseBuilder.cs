using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Reusable.Exceptionizer;
using Reusable.IOnymous;

namespace Reusable.Teapot
{
    public interface IResponseBuilder
    {
        [NotNull]
        UriString Uri { get; }

        [NotNull]
        SoftString Method { get; }

        [NotNull]
        ResponseBuilder Enqueue(Func<HttpRequest, ResponseInfo> next);

        [NotNull]
        ResponseInfo Next(HttpRequest request);
    }
    
    public class ResponseBuilder : IResponseBuilder
    {
        private readonly Queue<Func<HttpRequest, ResponseInfo>> _responses = new Queue<Func<HttpRequest, ResponseInfo>>();

        public ResponseBuilder(UriString uri, SoftString method)
        {
            Uri = uri;
            Method = method;
        }

        public UriString Uri { get; }

        public SoftString Method { get; }

        public ResponseBuilder Enqueue(Func<HttpRequest, ResponseInfo> next)
        {
            _responses.Enqueue(next);
            return this;
        }

        public ResponseInfo Next(HttpRequest request)
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