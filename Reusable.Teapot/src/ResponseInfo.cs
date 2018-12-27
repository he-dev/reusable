using System;
using JetBrains.Annotations;

namespace Reusable.Teapot
{
    public class ResponseInfo : IDisposable
    {
        public ResponseInfo(int statusCode, object content)
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