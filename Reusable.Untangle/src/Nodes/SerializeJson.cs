using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Reusable.Translucent.Abstractions;
using Reusable.Translucent.Data;
using Reusable.Utilities.JsonNet.Converters;
using Reusable.Utilities.JsonNet.Extensions;

namespace Reusable.Translucent.Nodes;

[UsedImplicitly]
public class SerializeJson<T> : ResourceNode where T : Request, IJsonRequest
{
    public JsonSerializer RequestSerializer { get; set; } = new()
    {
        Converters =
        {
            new SoftStringConverter(),
            new StringEnumConverter(),
            new ColorConverter(),
        }
    };

    public JsonSerializer? ResponseSerializer { get; set; }

    public override async Task InvokeAsync(ResourceContext context)
    {
        if (context.Request is T { Body: { } requestBody })
        {
            context.Request.Body.Push(RequestSerializer.Serialize(requestBody));
        }

        await InvokeNext(context);

        if (context.Request is T jsonRequest && context.Response is { } response && response.Body.Peek() is Stream stream)
        {
            var result = (ResponseSerializer ?? RequestSerializer).Deserialize(stream, jsonRequest.BodyType);
            if (result is { })
            {
                context.Response.Body.Push(result);
            }
        }
    }
}