using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Reusable.Flexo
{
    public static class ExpressionSerializerExtensions
    {
        [Obsolete("Use DeserializeExpressionArrayAsync")]
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
        
        [ItemNotNull]
        public static Task<IList<IExpression>> DeserializeExpressionArrayAsync(this IExpressionSerializer serializer, Stream jsonStream)
        {
            return serializer.DeserializeAsync<IList<IExpression>>(jsonStream);
        }

        [ContractAnnotation("jsonStream: null => halt")]
        public static T Deserialize<T>(this IExpressionSerializer serializer, Stream jsonStream)
        {
            return serializer.DeserializeAsync<T>(jsonStream).GetAwaiter().GetResult();
        }
    }
}