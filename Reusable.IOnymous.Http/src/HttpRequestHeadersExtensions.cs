using System.Net.Http.Headers;

namespace Reusable.IOnymous
{
    public static class HttpRequestHeadersExtensions
    {
        public static HttpRequestHeaders AcceptJson(this HttpRequestHeaders headers)
        {
            headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return headers;
        }

        public static HttpRequestHeaders AcceptHtml(this HttpRequestHeaders headers)
        {
            headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
            return headers;
        }

        public static HttpRequestHeaders UserAgent(this HttpRequestHeaders headers, string productName, string productVersion)
        {
            headers.UserAgent.Add(new ProductInfoHeaderValue(productName, productVersion));
            return headers;
        }
        
        public static HttpRequestHeaders ApiVersion(this HttpRequestHeaders headers, string apiVersion)
        {
            headers.Add("Api-Version", apiVersion);
            return headers;
        }   
    }
}