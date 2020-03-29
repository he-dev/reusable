using System;
using System.Collections.Immutable;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Reusable.Utilities.JsonNet.Converters;
using Reusable.Utilities.JsonNet.Services;
using Reusable.Utilities.JsonNet.Visitors;

namespace Reusable.Utilities.JsonNet
{
    public interface IPrettyJsonSerializer
    {
        T Deserialize<T>(string json, IImmutableDictionary<string, Type> knownTypes);
    }

    [PublicAPI]
    public class PrettyJsonSerializer : IPrettyJsonSerializer
    {
        private readonly JsonSerializer jsonSerializer;

        public PrettyJsonSerializer(IContractResolver contractResolver, Action<JsonSerializer>? configure = default)
        {
            jsonSerializer = new JsonSerializer
            {
                NullValueHandling = NullValueHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Auto,
                ObjectCreationHandling = ObjectCreationHandling.Replace,
                ContractResolver = contractResolver,
                Converters =
                {
                    new SoftStringConverter(),
                }
            };
            configure?.Invoke(jsonSerializer);
        }

        public T Deserialize<T>(string json, IImmutableDictionary<string, Type> knownTypes)
        {
            var visitor = new CompositeJsonVisitor
            {
                new TrimPropertyName(),
                new RewriteTypeVisitor(new NormalizePrettyTypeString(knownTypes))
            };
            var token = visitor.Visit(JToken.Parse(json));
            return token.ToObject<T>(jsonSerializer);
        }
    }

    public interface IConvertPrettyJson
    {
        JToken Read(string json, IImmutableDictionary<string, Type> knownTypes);
    }

    public class ConvertPrettyJson : IConvertPrettyJson
    {
        public JToken Read(string json, IImmutableDictionary<string, Type> knownTypes)
        {
            var visitor = new CompositeJsonVisitor
            {
                new TrimPropertyName(),
                new RewriteTypeVisitor(new NormalizePrettyTypeString(knownTypes))
            };
            return visitor.Visit(JToken.Parse(json));
        }
    }

    public static class PrettyJsonSerializerExtensions
    {
        public static async Task<T> DeserializeAsync<T>(this IPrettyJsonSerializer serializer, Stream jsonStream, IImmutableDictionary<string, Type> knownTypes)
        {
            return serializer.Deserialize<T>(await ReadJsonAsync(jsonStream), knownTypes);
        }

        private static async Task<string> ReadJsonAsync(Stream jsonStream)
        {
            using var streamReader = new StreamReader(jsonStream);
            return await streamReader.ReadToEndAsync();
        }
    }
}