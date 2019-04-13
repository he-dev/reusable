using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Newtonsoft.Json.Serialization;
using Reusable.Extensions;
using Reusable.Flexo;
using Reusable.IOnymous;
using Reusable.OmniLog;
using Reusable.OmniLog.Abstractions;
using Reusable.Utilities.JsonNet;
using Reusable.Utilities.JsonNet.DependencyInjection;
using Xunit;
using Double = Reusable.Flexo.Double;
using ExpressionSerializer = Reusable.Flexo.ExpressionSerializer;

// ReSharper disable once CheckNamespace
namespace Reusable.Tests.Flexo
{
    public class ExpressionSerializerTest : IDisposable
    {
        private static readonly IResourceProvider Flexo =
            EmbeddedFileProvider<ExpressionSerializerTest>
                .Default
                .DecorateWith(RelativeProvider.Factory(@"res\Flexo"));

        private readonly IExpressionSerializer _serializer;

        private readonly IDisposable _container;

        public ExpressionSerializerTest()
        {
            var builder = new ContainerBuilder();
            
            builder.RegisterModule<JsonContractResolverModule>();
            builder.RegisterModule(new LoggerModule(new LoggerFactory()));           
            builder.RegisterModule(new ExpressionSerializerModule(Expression.Types));

            var container = builder.Build();
            var scope = container.BeginLifetimeScope();

            _serializer = scope.Resolve<ExpressionSerializer.Factory>()(TypeDictionary.From(Expression.Types));
            _container = Disposable.Create(() =>
            {
                scope.Dispose();
                container.Dispose();
            });
        }

        [Fact]
        public async Task Can_deserialize_array_of_expressions()
        {
            var flexoFile = await Flexo.ReadTextFileAsync("Array-of-expressions.json");
            var expressions = _serializer.Deserialize<List<IExpression>>(flexoFile);

            //ExpressionAssert.Equal(Constant.True, expressions.OfType<Any>().Single());
            //ExpressionAssert.Equal(new double[] { 1, 2, 3 }, expressions.Get("CollectionMixed"));
            ExpressionAssert.Equal(Constant.True, expressions.Get("AnyWithPredicate"));
            ExpressionAssert.Equal(Constant.True, expressions.Get("AnyWithoutPredicate"));
            ExpressionAssert.Equal(Constant.True, expressions.Get("AllWithPredicate"));
            //ExpressionAssert.Equal(Constant.Create(3.0), expressions.OfType<Sum>().Single());
            //ExpressionAssert.Equal(Double.One, expressions.OfType<SwitchToDouble>().Single());
            ExpressionAssert.Equal(Constant.False, expressions.Get("NotExtension"));
            ExpressionAssert.Equal(Double.One, expressions.Get("ToDoubleExtension"));
            //ExpressionAssert.Equal(Double.One, expressions.Get("SwitchToDoubleInvalid"));
            ExpressionAssert.Equal(3.0, expressions.Get("SumExtension"));
            ExpressionAssert.Equal(4.0, expressions.Get("MaxExtension"));
            ExpressionAssert.Equal(Constant.True, expressions.Get("ContainsExtension"));
            ExpressionAssert.Equal(Constant.True, expressions.Get("Matches"));
            ExpressionAssert.Equal(Constant.True, expressions.Get("CollectionWithRegexComparer"));
            ExpressionAssert.Equal(Constant.True, expressions.Get("CollectionWithAnyMatches"));
            ExpressionAssert.Equal(Constant.True, expressions.Get("CollectionWithAll"));
            ExpressionAssert.Equal(Constant.True, expressions.Get("CollectionWithOverlaps"));
            ExpressionAssert.Equal(Constant.True, expressions.Get("DoubleWithIsEqual"));
            var collectionOfDouble = (Reusable.Flexo.Collection)expressions.Get("CollectionOfDouble");
            Assert.Equal(2, collectionOfDouble.Values.Count);
            ExpressionAssert.Equal(1.0, collectionOfDouble.Values[0]);
            ExpressionAssert.Equal(2.0, collectionOfDouble.Values[1]);

            var collectionWithSelect = expressions.Get("CollectionWithSelect").Invoke(ExpressionContext.Empty).Value<List<object>>();
            Assert.Equal(new[] { "1", "True" }, collectionWithSelect);
        }

        [Fact]
        public async Task Can_deserialize_single_expression()
        {
            var jsonFile = await Flexo.GetFileAsync(@"Single-expression.json", MimeType.Json);
            using (var jsonStream = await jsonFile.CopyToMemoryStreamAsync())
            {
                var expression = await _serializer.DeserializeExpressionAsync(jsonStream.Rewind());
                Assert.NotNull(expression);
            }
        }

        public void Dispose()
        {
            _container.Dispose();
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