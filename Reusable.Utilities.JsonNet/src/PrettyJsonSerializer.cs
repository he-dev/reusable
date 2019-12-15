using System;
using System.Collections.Immutable;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Reusable.Utilities.JsonNet.Converters;

namespace Reusable.Utilities.JsonNet
{
    public interface IPrettyJsonSerializer
    {
        T Deserialize<T>(string json, IImmutableDictionary<SoftString, Type> knownTypes);
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

        public T Deserialize<T>(string json, IImmutableDictionary<SoftString, Type> knownTypes)
        {
            var visitor = new CompositeJsonVisitor
            {
                new TrimPropertyNameVisitor(),
                new RewriteTypeVisitor(new PrettyTypeResolver(knownTypes))
            };
            var token = visitor.Visit(JToken.Parse(json));
            return token.ToObject<T>(jsonSerializer);
        }
    }

    public static class PrettyJsonSerializerExtensions
    {
        public static async Task<T> DeserializeAsync<T>(this IPrettyJsonSerializer serializer, Stream jsonStream, IImmutableDictionary<SoftString, Type> knownTypes)
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