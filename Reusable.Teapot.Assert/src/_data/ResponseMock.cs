using System;
using JetBrains.Annotations;

namespace Reusable.Teapot
{
    public class ResponseMock : IDisposable
    {
        public ResponseMock(int statusCode, object? content, string contentType)
        {
            StatusCode = statusCode;
            Content = content;
            ContentType = contentType;
        }

        public int StatusCode { get; }

        public object? Content { get; }

        public string ContentType { get; }

        public void Dispose()
        {
            (Content as IDisposable)?.Dispose();
        }
    }
}