using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Reusable.Marbles.Extensions;
using Reusable.Utilities.JsonNet;
using Reusable.Utilities.JsonNet.Abstractions;
using Reusable.Utilities.JsonNet.Visitors;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Extensions;

namespace Reusable.Utilities.AspNetCore.Middleware;

[UsedImplicitly]
public class NormalizeJsonTypePropertyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger _logger;
    private readonly IJsonVisitor _normalize;

    public NormalizeJsonTypePropertyMiddleware
    (
        RequestDelegate next,
        ILogger<NormalizeJsonTypePropertyMiddleware> logger,
        ITypeDictionary typeDictionary
    )
    {
        _next = next;
        _logger = logger;
        _normalize = new CompositeJsonVisitor
        {
            new TrimPropertyName(),
            new NormalizeTypeProperty(typeDictionary)
        };
    }

    public async Task Invoke(HttpContext context)
    {
        if (context.Request.ContentType == "application/json" && context.Request.ContentLength > 0)
        {
            using var scope = _logger.BeginUnitOfWork("NormalizeJson");
            try
            {
                using var requestReader = new HttpRequestStreamReader(context.Request.Body, Encoding.UTF8);
                using var jsonReader = new JsonTextReader(requestReader);

                var json = await JToken.LoadAsync(jsonReader);
                var normalized = _normalize.Visit(json);

                var memoryStream = new MemoryStream();
                var requestWriter = new StreamWriter(memoryStream);
                var jsonWriter = new JsonTextWriter(requestWriter);

                await normalized.WriteToAsync(jsonWriter);
                await jsonWriter.FlushAsync();

                var content = new StreamContent(memoryStream.Rewind());
                context.Request.Body = await content.ReadAsStreamAsync();
            }
            catch (Exception e)
            {
                scope.SetException(e);
            }
        }

        await _next(context);
    }
}