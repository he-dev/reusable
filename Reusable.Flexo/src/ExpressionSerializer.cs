using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Utilities.JsonNet;

namespace Reusable.Flexo
{
    public interface IExpressionSerializer
    {
        [NotNull]
        T Deserialize<T>([NotNull] Stream jsonStream);
    }

    public class ExpressionSerializer : IExpressionSerializer
    {
        private static readonly IEnumerable<Type> OwnTypes = new[]
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
            typeof(IIf),
            typeof(Min),
            typeof(Max),
            typeof(Select),
            typeof(Sum)
        };

        private readonly JsonSerializer _jsonSerializer;

        private readonly IEnumerable<Type> _jsonTypes;

        public ExpressionSerializer(IEnumerable<Type> otherTypes = null, Action<JsonSerializer> configureSerializer = null)
        {
            _jsonSerializer = new JsonSerializer
            {
                NullValueHandling = NullValueHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Auto,
                //ContractResolver = contractResolver,
            };
            _jsonTypes = OwnTypes.Concat(otherTypes ?? Enumerable.Empty<Type>()).ToList();
            configureSerializer?.Invoke(_jsonSerializer);
        }

        [ContractAnnotation("jsonStream: null => halt")]
        public T Deserialize<T>(Stream jsonStream)
        {
            if (jsonStream == null) throw new ArgumentNullException(nameof(jsonStream));

            using (var streamReader = new StreamReader(jsonStream))
            using (var jsonTextReader = new PrettyTypeReader(streamReader, PrettyTypeReader.AbbreviatedTypePropertyName, PrettyTypeResolver.Create(_jsonTypes)))
            {
                return _jsonSerializer.Deserialize<T>(jsonTextReader);
            }
        }
    }
}
