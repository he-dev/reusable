using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Reusable.Extensions;
using Reusable.Flexo;
using Reusable.IOnymous;
using Xunit;

// ReSharper disable once CheckNamespace
namespace Reusable.Tests.Flexo
{
    using static Assert;

    public class ExpressionSerializerTest
    {
        private static readonly ExpressionSerializer Serializer = new ExpressionSerializer();

        private static readonly IResourceProvider Flexo = EmbeddedFileProvider<ExpressionSerializerTest>.Default.DecorateWith(RelativeProvider.Factory(@"res\Flexo"));

        [Fact]
        public async Task Can_deserialize_array_of_expressions()
        {
            var jsonFile = await Flexo.GetFileAsync("Array-of-expressions.json", MimeType.Json);
            using (var memoryStream = await jsonFile.CopyToMemoryStreamAsync())
            {
                var expressions = await Serializer.DeserializeExpressionsAsync(memoryStream.Rewind());

                Equal(4, expressions.Count);
                
                ExpressionAssert.Equal(Constant.True, expressions.OfType<Any>().Single());
                ExpressionAssert.Equal(Constant.False, expressions.OfType<All>().Single());
                ExpressionAssert.Equal(Constant.Create(3.0), expressions.OfType<Sum>().Single());
                ExpressionAssert.Equal(Double.One, expressions.OfType<SwitchBooleanToDouble>().Single());
            }
        }

        [Fact]
        public async Task Can_deserialize_single_expression()
        {
            var jsonFile = await Flexo.GetFileAsync(@"Single-expression.json", MimeType.Json);
            using (var jsonStream = await jsonFile.CopyToMemoryStreamAsync())
            {
                var expression = await Serializer.DeserializeExpressionAsync(jsonStream.Rewind());
                NotNull(expression);
            }
        }                
    }

    internal class MyTest
    {
        public IExpression MyExpression { get; set; }
    }
}