using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Reusable.Utilities.JsonNet;

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
        private readonly IJsonVisitor _transform;

        private readonly JsonSerializer _jsonSerializer;

        public delegate IExpressionSerializer Factory([NotNull] IEnumerable<Type> customTypes, [CanBeNull] Action<JsonSerializer> configureSerializer = null);

        public ExpressionSerializer
        (
            [NotNull] IEnumerable<Type> customTypes,
            [NotNull] IContractResolver contractResolver,
            [CanBeNull] Action<JsonSerializer> configureSerializer = null
        )
        {
            if (customTypes == null) throw new ArgumentNullException(nameof(customTypes));
            var types =
                TypeDictionary
                    .BuiltInTypes
                    .AddRange(TypeDictionary.From(Expression.Types))
                    .AddRange(TypeDictionary.From(customTypes));

            _transform =
                CompositeJsonVisitor
                    .Empty
                    .Add(new TrimPropertyNameVisitor())
                    .Add(new RewritePrettyTypeVisitor(types));

            _jsonSerializer = new JsonSerializer
            {
                NullValueHandling = NullValueHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Auto,
                //DefaultValueHandling = DefaultValueHandling.Ignore,
                ContractResolver = contractResolver,
                Converters =
                {
                    //new ExpressionConverter(),
                    new ExpressionConverter()
                }
            };

            configureSerializer?.Invoke(_jsonSerializer);
        }

//        public ExpressionSerializer([NotNull] IEnumerable<Type> customTypes, [CanBeNull] Action<JsonSerializer> configureSerializer = null)       
//        {
//            if (customTypes == null) throw new ArgumentNullException(nameof(customTypes));
//            var types =
//                TypeDictionary
//                    .BuiltInTypes
//                    .AddRange(TypeDictionary.From(Expression.Types))
//                    .AddRange(TypeDictionary.From(customTypes));
//
//            _transform =
//                CompositeJsonVisitor
//                    .Empty
//                    .Add(new TrimPropertyNameVisitor())
//                    .Add(new RewritePrettyTypeVisitor(types));
//
//            _jsonSerializer = new JsonSerializer
//            {
//                NullValueHandling = NullValueHandling.Ignore,
//                TypeNameHandling = TypeNameHandling.Auto,
//                //DefaultValueHandling = DefaultValueHandling.Ignore,
//                //ContractResolver = contractResolver,
//                Converters =
//                {
//                    //new ExpressionConverter(),
//                    new ExpressionConverter()
//                }
//            };
//
//            configureSerializer?.Invoke(_jsonSerializer);
//        }

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
}