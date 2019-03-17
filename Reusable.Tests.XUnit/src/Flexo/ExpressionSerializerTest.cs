using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Reusable.Extensions;
using Reusable.Flexo;
using Reusable.IOnymous;
using Xunit;
using Double = Reusable.Flexo.Double;

// ReSharper disable once CheckNamespace
namespace Reusable.Tests.Flexo
{    
    public class ExpressionSerializerTest
    {
        private static readonly ExpressionSerializer Serializer = new ExpressionSerializer(Enumerable.Empty<Type>());

        private static readonly IResourceProvider Flexo = 
            EmbeddedFileProvider<ExpressionSerializerTest>
                .Default
                .DecorateWith(RelativeProvider.Factory(@"res\Flexo"));

        [Fact]
        public async Task Can_deserialize_array_of_expressions()
        {
            var jsonFile = await Flexo.GetFileAsync("Array-of-expressions.json", MimeType.Json);
            using (var memoryStream = await jsonFile.CopyToMemoryStreamAsync())
            {
                var expressions = await Serializer.DeserializeExpressionsAsync(memoryStream.Rewind());

                //Assert.Equal(8, expressions.Count);
                
                //ExpressionAssert.Equal(Constant.True, expressions.OfType<Any>().Single());
                //ExpressionAssert.Equal(Constant.False, expressions.OfType<All>().Single());
                //ExpressionAssert.Equal(Constant.Create(3.0), expressions.OfType<Sum>().Single());
                //ExpressionAssert.Equal(Double.One, expressions.OfType<SwitchToDouble>().Single());
                ExpressionAssert.Equal(Constant.False, expressions.Get("NotExtension"));
                ExpressionAssert.Equal(Double.One, expressions.Get("SwitchToDoubleExtension"));
                //ExpressionAssert.Equal(Double.One, expressions.Get("SwitchToDoubleInvalid"));
                ExpressionAssert.Equal(3.0, expressions.Get("SumExtension"));
                ExpressionAssert.Equal(4.0, expressions.Get("MaxExtension"));
                ExpressionAssert.Equal(Constant.True, expressions.Get("ContainsExtension"));
            }
        }

        [Fact]
        public async Task Can_deserialize_single_expression()
        {
            var jsonFile = await Flexo.GetFileAsync(@"Single-expression.json", MimeType.Json);
            using (var jsonStream = await jsonFile.CopyToMemoryStreamAsync())
            {
                var expression = await Serializer.DeserializeExpressionAsync(jsonStream.Rewind());
                Assert.NotNull(expression);
            }
        }                
    }

    internal static class EnumerableExtensions
    {
        public static IExpression Get(this IEnumerable<IExpression> expressions, SoftString name)
        {
            return expressions.Single(e => e.Name == name);
        }
    }
}