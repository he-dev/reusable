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
        private static readonly Type[] OwnTypes =
        {
            //typeof(Reusable.Flexo.Equal<>),
            typeof(Reusable.Flexo.ObjectEqual),
            typeof(Reusable.Flexo.StringEqual),
            typeof(Reusable.Flexo.GreaterThan),
            typeof(Reusable.Flexo.GreaterThanOrEqual),
            typeof(Reusable.Flexo.LessThan),
            typeof(Reusable.Flexo.LessThanOrEqual),
            typeof(Reusable.Flexo.Not),
            typeof(Reusable.Flexo.All),
            typeof(Reusable.Flexo.Any),
            //typeof(Reusable.Flexo.Average),
            typeof(Reusable.Flexo.String),
            typeof(Reusable.Flexo.IIf),
            typeof(Reusable.Flexo.Switch),
            typeof(Reusable.Flexo.SwitchBooleanToDouble),
            typeof(Reusable.Flexo.GetContextItem),
            typeof(Reusable.Flexo.Contains),
            typeof(Reusable.Flexo.Min),
            typeof(Reusable.Flexo.Max),
            typeof(Reusable.Flexo.Sum),
            typeof(Reusable.Flexo.ToList),
            typeof(Reusable.Flexo.Constant<>),
            typeof(Reusable.Flexo.Double),
            typeof(Reusable.Flexo.Integer),
            typeof(Reusable.Flexo.Decimal),
            typeof(Reusable.Flexo.Instant),
            typeof(Reusable.Flexo.Interval),
            typeof(Reusable.Flexo.True),
            typeof(Reusable.Flexo.False),
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
                new PrettyTypeResolver(OwnTypes.Concat(customTypes ?? Enumerable.Empty<Type>()))
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