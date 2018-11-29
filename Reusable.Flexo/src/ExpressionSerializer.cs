using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
            typeof(Select),
            typeof(Sum),
        };

        private readonly VisitJsonCallback Transform;

        private readonly JsonSerializer _jsonSerializer;       

        public ExpressionSerializer(IEnumerable<Type> customTypes = null, Action<JsonSerializer> configureSerializer = null)
        {
            _jsonSerializer = new JsonSerializer
            {
                NullValueHandling = NullValueHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Auto,
                //ContractResolver = contractResolver,
            };

            Transform = JsonVisitor.Create
            (
                new PropertyNameTrimmer(),
                new PrettyTypeResolver(InternalTypes.Concat(customTypes ?? Enumerable.Empty<Type>()))
            );

            configureSerializer?.Invoke(_jsonSerializer);
        }

        [ContractAnnotation("jsonStream: null => halt")]
        public T Deserialize<T>(Stream jsonStream)
        {
            if (jsonStream == null) throw new ArgumentNullException(nameof(jsonStream));

            using (var streamReader = new StreamReader(jsonStream))
            {
                var json = streamReader.ReadToEnd();
                var token = JToken.Parse(json);
                return Transform(token).ToObject<T>(_jsonSerializer);
            }
        }
    }
}
