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
        private readonly IContractResolver _contractResolver;
        private readonly JsonSerializer _jsonSerializer;

        public PrettyJsonSerializer
        (
            IContractResolver contractResolver,
            Action<JsonSerializer>? configureSerializer = default
        )
        {
            _contractResolver = contractResolver ?? throw new ArgumentNullException(paramName: nameof(contractResolver), message: $"You need to register na {nameof(IContractResolver)}.");

            _jsonSerializer = new JsonSerializer
            {
                NullValueHandling = NullValueHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Auto,
                ContractResolver = contractResolver,
                Converters =
                {
                    new SoftStringConverter(),
                }
            };

            configureSerializer?.Invoke(_jsonSerializer);
        }

        public T Deserialize<T>(string json, IImmutableDictionary<SoftString, Type> knownTypes)
        {
            if (json == null) throw new ArgumentNullException(nameof(json));

            return
                ImmutableList<IJsonVisitor>
                    .Empty
                    .Add(new TrimPropertyNameVisitor())
                    .Add(new RewriteTypeVisitor(new PrettyTypeResolver(knownTypes)))
                    .Visit(JToken.Parse(json))
                    .ToObject<T>(_jsonSerializer);
        }
    }

    public static class PrettyJsonSerializerExtensions
    {
        public static async Task<T> DeserializeAsync<T>(this IPrettyJsonSerializer serializer, Stream jsonStream, IImmutableDictionary<SoftString, Type> knownTypes)
        {
            if (jsonStream == null) throw new ArgumentNullException(nameof(jsonStream));

            return serializer.Deserialize<T>(await ReadJsonAsync(jsonStream), knownTypes);
        }

        private static async Task<string> ReadJsonAsync(Stream jsonStream)
        {
            using (var streamReader = new StreamReader(jsonStream))
            {
                return await streamReader.ReadToEndAsync();
            }
        }
    }
}