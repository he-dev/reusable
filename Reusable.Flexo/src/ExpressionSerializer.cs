using System;
using System.Collections.Immutable;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Reusable.Flexo.Json;
using Reusable.Utilities.JsonNet;
using Reusable.Utilities.JsonNet.Converters;
using Reusable.Utilities.JsonNet.Services;
using Reusable.Utilities.JsonNet.Visitors;

namespace Reusable.Flexo
{
    public interface IExpressionSerializer
    {
        [ItemNotNull]
        Task<T> DeserializeAsync<T>([NotNull] Stream jsonStream);

        [NotNull]
        T Deserialize<T>([NotNull] string json);
    }

    [PublicAPI]
    public class ExpressionSerializer : IExpressionSerializer
    {
        [NotNull]
        private readonly IContractResolver _contractResolver;

        public ExpressionSerializer
        (
            IImmutableDictionary<string, Type> expressionTypes,
            IContractResolver contractResolver,
            Action<JsonSerializer>? configureSerializer = null
        )
        {
            _contractResolver = contractResolver;

            Transform = new CompositeJsonVisitor
            {
                new TrimPropertyName(),
                new RewriteTypeVisitor(new NormalizePrettyTypeString(expressionTypes)),
            };

            JsonSerializer = new JsonSerializer
            {
                NullValueHandling = NullValueHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Auto,
                ObjectCreationHandling = ObjectCreationHandling.Replace,
                ContractResolver = contractResolver,
                Converters =
                {
                    //new ExpressionConverter(),
                    new SoftStringConverter(),
                    new ExpressionConverter()
                }
            };

            configureSerializer?.Invoke(JsonSerializer);
        }

        public IJsonVisitor Transform { get; set; }

        public JsonSerializer JsonSerializer { get; set; }

        public async Task<T> DeserializeAsync<T>(Stream jsonStream)
        {
            return Deserialize<T>(await ReadJsonAsync(jsonStream));
        }

        public T Deserialize<T>(string json)
        {
            return Transform.Visit(json).ToObject<T>(JsonSerializer);
        }

        private static async Task<string> ReadJsonAsync(Stream jsonStream)
        {
            using var streamReader = new StreamReader(jsonStream);
            return await streamReader.ReadToEndAsync();
        }
    }
}