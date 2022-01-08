using System;
using System.Net.Http.Headers;

namespace Reusable.Synergy.Requests;

public interface IHttpRequest : IRequest
{
    public string Uri { get; }

    public object? Body { get; }

    public ProductInfoHeaderValue? UserAgent { get; }

    public Action<HttpRequestHeaders> HeadersConfiguration { get; }

    //public MediaTypeFormatter? RequestFormatter { get; set; }

    public string ContentType { get; }
}

public abstract class HttpRequest<T> : Request<T>, IHttpRequest
{
    protected HttpRequest(string uri)
    {
        Uri = uri;
        this.AcceptJson();
    }

    public string Uri { get; }

    public object? Body { get; set; }

    public ProductInfoHeaderValue? UserAgent { get; set; }

    public Action<HttpRequestHeaders> HeadersConfiguration { get; set; } = _ => { };

    //public MediaTypeFormatter? RequestFormatter { get; set; }

    public string ContentType { get; set; } = "application/json";

    public class Get : HttpRequest<T>
    {
        public Get(string uri) : base(uri) { }
    }

    public class Post : HttpRequest<T>
    {
        public Post(string uri) : base(uri) { }
    }

    public class Put : HttpRequest<T>
    {
        public Put(string uri) : base(uri) { }
    }

    public class Delete : HttpRequest<T>
    {
        public Delete(string uri) : base(uri) { }
    }
}