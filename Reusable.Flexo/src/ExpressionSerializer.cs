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
            IImmutableDictionary<SoftString, Type> expressionTypes,
            IContractResolver contractResolver,
            Action<JsonSerializer>? configureSerializer = null
        )
        {
            if (expressionTypes == null) throw new ArgumentNullException(nameof(expressionTypes));
            _contractResolver = contractResolver; // ?? throw new ArgumentNullException(paramName: nameof(contractResolver), message: $"You need to register na {nameof(IContractResolver)}.");

            Transform =
                CompositeJsonVisitor
                    .Empty
                    .Add(new TrimPropertyNameVisitor())
                    .Add(new RewriteTypeVisitor(new PrettyTypeResolver(expressionTypes)));

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

        [ContractAnnotation("jsonStream: null => halt")]
        public async Task<T> DeserializeAsync<T>(Stream jsonStream)
        {
            if (jsonStream == null) throw new ArgumentNullException(nameof(jsonStream));
            return Deserialize<T>(await ReadJsonAsync(jsonStream));
        }

        [ContractAnnotation("json: null => halt")]
        public T Deserialize<T>(string json)
        {
            if (json == null) throw new ArgumentNullException(nameof(json));
            return Transform.Visit(json).ToObject<T>(JsonSerializer);
        }

        private static async Task<string> ReadJsonAsync(Stream jsonStream)
        {
            using var streamReader = new StreamReader(jsonStream);
            return await streamReader.ReadToEndAsync();
        }
    }
}