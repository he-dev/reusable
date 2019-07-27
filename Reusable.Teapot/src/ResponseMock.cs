using System;
using JetBrains.Annotations;

namespace Reusable.Teapot
{
    public class ResponseMock : IDisposable
    {
        public ResponseMock(int statusCode, object content)
        {
            StatusCode = statusCode;
            Content = content;
        }

        public int StatusCode { get; }

        [CanBeNull]
        public object Content { get; }

        public void Dispose()
        {
            (Content as IDisposable)?.Dispose();
        }
    }
}