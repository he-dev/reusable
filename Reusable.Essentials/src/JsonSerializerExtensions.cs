using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace System.Text.Json.Custom;

public static class JsonSerializerExtensions
{
    public static T? DeserializeAnonymousType<T>(string json, T anonymousTypeObject, JsonSerializerOptions? options = default)
    {
        return JsonSerializer.Deserialize<T>(json, options);
    }
    
    public static ValueTask<TValue?> DeserializeAnonymousTypeAsync<TValue>(Stream stream, TValue anonymousTypeObject, JsonSerializerOptions? options = default, CancellationToken cancellationToken = default)
    {
        return JsonSerializer.DeserializeAsync<TValue>(stream, options, cancellationToken);
    }
}