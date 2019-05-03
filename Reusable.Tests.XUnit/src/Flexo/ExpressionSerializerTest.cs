using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net.PeerToPeer;
using System.Threading.Tasks;
using Autofac;
using Reusable.Data;
using Reusable.Flexo;
using Reusable.IOnymous;
using Reusable.OmniLog;
using Reusable.Utilities.JsonNet;
using Reusable.Utilities.JsonNet.DependencyInjection;
using Reusable.Utilities.XUnit.Annotations;
using Xunit;
using Xunit.Abstractions;
using Double = Reusable.Flexo.Double;
using ExpressionSerializer = Reusable.Flexo.ExpressionSerializer;

// ReSharper disable once CheckNamespace
namespace Reusable.Tests.Flexo
{
    public class ExpressionSerializerTest : IClassFixture<ExpressionFixture>
    {
        private readonly ExpressionFixture _helper;
        private readonly ITestOutputHelper _output;

        private static readonly IResourceProvider Flexo =
            EmbeddedFileProvider<ExpressionSerializerTest>
                .Default
                .DecorateWith(RelativeProvider.Factory(@"res\Flexo"));

        public ExpressionSerializerTest(ExpressionFixture helper, ITestOutputHelper output)
        {
            _helper = helper;
            _output = output;
        }

        [Theory]
        [SmartMemberData(nameof(GetData))]
        public void Can_evaluate_supported_expressions(string useCaseName, object expected, bool throws)
        {
            var useCase = _helper.GetExpressions().Single(e => e.Name == useCaseName);
            ExpressionAssert.Equal(expected, useCase, ctx => ctx.WithReferences(_helper.GetReferences()), _output, throws);
        }

        [Fact]
        public async Task Can_deserialize_single_expression()
        {
            var jsonFile = await Flexo.GetFileAsync(@"ExpressionObject.json", MimeType.Json);
            using (var jsonStream = await jsonFile.CopyToMemoryStreamAsync())
            {
                var expression = await _helper.Serializer.DeserializeExpressionAsync(jsonStream.Rewind());
                Assert.NotNull(expression);
            }
        }

        private static IEnumerable<object> GetData() => new (string UseCaseName, object Expected, bool Throws)[]
        {
            ("Any", true, false),
            ("Any{Predicate=false}", true, false),
            ("All{Predicate=true}", false, false),
            ("Sum", 3.0, false),
            ("ToDouble", 1.0, false),
            ("Matches", true, false),
            ("True.Not", false, false),
            ("True.ToDouble", 1.0, false),
            ("Double.ToDouble", 1.0, true),
            ("Double.IsEqual", true, false),
            ("Double.ref_IsLessThan3", true, false),
            ("CollectionOfDouble", new double[] { 1, 2, 3 }, false),
            ("Collection.Sum", 3.0, false),
            ("Collection.Max", 4.0, false),
            ("Collection.Contains{Comparer=Regex}", true, false),
            ("Collection.Contains{Comparer=Regex}", true, false),
            ("Collection.Any{Predicate=Matches}", true, false),
            ("Collection.Contains", true, false),
            ("Collection.All", true, false),
            ("Collection.Overlaps{Comparer=SoftString}", true, false),
            ("Collection.Select.ToString", new[] { "1", "True" }, false),
            ("String.In", true, false)
        }.Select(uc => new { uc.UseCaseName, uc.Expected, uc.Throws });
    }
}