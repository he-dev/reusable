using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Reusable.sdk.Http;
using Reusable.sdk.Http.Formatting;
using Reusable.sdk.Mailr;

namespace Reusable.Apps.Demos
{
    public static class RestClientDemo
    {
        public static void Start()
        {
            var client = new RestClient("http://localhost:49471/api/", headers =>
            {
                headers.AcceptJson();
                headers.UserAgent(productName: "Reusable", productVersion: "7.0");
            });

            var context = new HttpMethodContext(HttpMethod.Get, "mailr", "messages", "test")
            {
                ResponseFormatters = { new TextMediaTypeFormatter() }
            };
            context.RequestHeadersActions.Add(headers => headers.AcceptHtml());
            var result = client.InvokeAsync<string>(context).GetAwaiter().GetResult();

            //var client = new RestClient("http://localhost:54245/api/", configureDefaultRequestHeaders);
        }

        public static async Task Mailr()
        {
            try
            {
                var mailr = RestClient.Create<IMailrClient>(baseUri: ConfigurationManager.AppSettings["mailr:BaseUri"], headers =>
                {
                    headers.AcceptJson();
                    headers.UserAgent(productName: "MailrNET", productVersion: "3.0.0");
                });

                var body =
                    await
                        mailr
                            .Resource("mailr", "messages", "test")
                            .SendAsync(Email.CreateHtml("...@gmail.com", "Testmail", new { Greeting = "Hallo Mailr!" }, email => email.CanSend = false));
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex);
            }
        }
    }

    //public interface ITests { }

    //public static class TestsClient
    //{
    //    public static IResource<ITests> Tests(this IRestClient client, string name) => client.Resource<ITests>(name);
    //}

    //public static class TestClientExtensions
    //{
    //    public static Task MessageAsync(this IResource<ITests> resource)
    //    {
    //        return resource.GetAsync<string>(CancellationToken.None);
    //    }
    //}
}
