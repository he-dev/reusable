using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Reusable.Utilities.JsonNet;

namespace Reusable.Flexo
{
    public interface IExpressionSerializer
    {
        [NotNull]
        Task<T> DeserializeAsync<T>([NotNull] Stream jsonStream);
    }

    public class ExpressionSerializer : IExpressionSerializer
    {
        private static readonly Type[] InternalTypes =
        {
            typeof(Equals),
            typeof(GreaterThan),
            typeof(GreaterThanOrEqual),
            typeof(LessThan),
            typeof(LessThanOrEqual),
            typeof(Not),
            typeof(All),
            typeof(Any),
            typeof(Constant<>),
            typeof(True),
            typeof(False),
            typeof(Zero),
            typeof(One),
            typeof(String),
            typeof(IIf),
            typeof(Min),
            typeof(Max),
            typeof(ToList),
            typeof(Sum),
        };

        private readonly JsonVisitor _transform;

        private readonly JsonSerializer _jsonSerializer;

        public ExpressionSerializer(IEnumerable<Type> customTypes = null, Action<JsonSerializer> configureSerializer = null)
        {
            _jsonSerializer = new JsonSerializer
            {
                NullValueHandling = NullValueHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Auto,
                //ContractResolver = contractResolver,
            };

            _transform = JsonVisitor.CreateComposite
            (
                new PropertyNameTrimmer(),
                new PrettyTypeResolver(InternalTypes.Concat(customTypes ?? Enumerable.Empty<Type>()))
            );

            configureSerializer?.Invoke(_jsonSerializer);
        }

        [ContractAnnotation("jsonStream: null => halt")]
        public async Task<T> DeserializeAsync<T>(Stream jsonStream)
        {
            if (jsonStream == null) throw new ArgumentNullException(nameof(jsonStream));

            var json = await ReadJsonAsync(jsonStream);
            return _transform.Visit(json).ToObject<T>(_jsonSerializer);
        }

        private static async Task<string> ReadJsonAsync(Stream jsonStream)
        {
            using (var streamReader = new StreamReader(jsonStream))
            {
                return await streamReader.ReadToEndAsync();
            }
        }
    }

    public static class ExpressionSerializerExtensions
    {
        [ItemNotNull]
        public static Task<IList<IExpression>> DeserializeExpressionsAsync(this IExpressionSerializer serializer, Stream jsonStream)
        {
            return serializer.DeserializeAsync<IList<IExpression>>(jsonStream);
        }

        [ItemNotNull]
        public static Task<IExpression> DeserializeExpressionAsync(this IExpressionSerializer serializer, Stream jsonStream)
        {
            return serializer.DeserializeAsync<IExpression>(jsonStream);
        }

        [ContractAnnotation("jsonStream: null => halt")]
        public static T Deserialize<T>(this IExpressionSerializer serializer, Stream jsonStream)
        {
            return serializer.DeserializeAsync<T>(jsonStream).GetAwaiter().GetResult();
        }
    }
}