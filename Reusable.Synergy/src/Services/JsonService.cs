using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Reusable.Utilities.JsonNet.Converters;
using Reusable.Utilities.JsonNet.Extensions;

namespace Reusable.Synergy.Services;

public class JsonService : Service
{
    public JsonService(IPropertyAccessor<object>? property = default) => Property = property;

    private IPropertyAccessor<object>? Property { get; }

    public JsonSerializer Serializer { get; set; } = new()
    {
        Converters =
        {
            new SoftStringConverter(),
            new StringEnumConverter(),
            new ColorConverter(),
        }
    };

    public override async Task<object> InvokeAsync(IRequest request)
    {
        // Serialize request.
        if (Property is { } property && property.GetValue(request) is { } value and not string and not Stream)
        {
            var serialized = new MemoryStream();
            Serializer.Serialize(serialized, value);
            property.SetValue(request, request);
        }

        var obj = await InvokeNext(request);

        // Deserialize response.
        if (Property is null && obj is Stream source)
        {
            return Serializer.Deserialize(source, request.GetType().GetGenericArguments().First());
        }

        return obj;
    }
}