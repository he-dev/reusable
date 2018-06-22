using System;
using System.Net.Http.Headers;

namespace Reusable.Net.Http
{
    public class HttpRequestHeadersConfiguration
    {
        private readonly Action<HttpRequestHeaders> _action;

        public HttpRequestHeadersConfiguration(Action<HttpRequestHeaders> action)
        {
            _action = action;
        }

        public HttpRequestHeadersConfiguration() : this(_ => { }) { }

        public void Apply(HttpRequestHeaders headers)
        {
            _action(headers);
        }

        public static implicit operator HttpRequestHeadersConfiguration(Action<HttpRequestHeaders> action)
        {
            return new HttpRequestHeadersConfiguration(action);
        }

        public static HttpRequestHeadersConfiguration operator +(HttpRequestHeadersConfiguration configuration, Action<HttpRequestHeaders> action)
        {
            return configuration._action.Append(action);
        }
    }
}