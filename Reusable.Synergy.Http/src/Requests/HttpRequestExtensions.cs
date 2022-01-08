using System;
using System.Net.Http.Headers;
using Reusable.Essentials;

namespace Reusable.Synergy.Requests;

public static class HttpRequestExtensions
{
    public static HttpRequest<T> AcceptJson<T>(this HttpRequest<T> request)
    {
        return request.Configure(headers => headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json")));
    }

    public static HttpRequest<T> AcceptHtml<T>(this HttpRequest<T> request)
    {
        return request.Configure(headers => headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html")));
    }

    public static HttpRequest<T> UserAgent<T>(this HttpRequest<T> request, string productName, string productVersion)
    {
        return request.Configure(headers => headers.UserAgent.Add(new ProductInfoHeaderValue(productName, productVersion)));
    }

    public static HttpRequest<T> ApiVersion<T>(this HttpRequest<T> request, string apiVersion)
    {
        return request.Configure(headers => headers.Add("Api-Version", apiVersion));
    }

    public static HttpRequest<T> CorrelationId<T>(this HttpRequest<T> request, string correlationId)
    {
        return request.Configure(headers => headers.Add("X-Correlation-ID", correlationId));
    }

    public static HttpRequest<T> Configure<T>(this HttpRequest<T> request, Action<HttpRequestHeaders> headersConfiguration)
    {
        return request.Also(r => r.HeadersConfiguration = r.HeadersConfiguration.Then(headersConfiguration));
    }
}