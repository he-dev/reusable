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
using Reusable.Quickey;
using Reusable.Tests.Flexo.Helpers;
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
            var sth = new Something();
            Assert.That.ExpressionEqual(expected, useCase, ctx => ctx.WithReferences(_helper.GetReferences()).SetItem(From<ITestMeta>.Select(x => x.Sth), sth), _output, throws);

            switch (useCaseName)
            {
                case "Concat":
                case "Union":
                    Assert.Equal(new[] { "Joe", "Bob", "Tom" }, sth.Names);
                    break;
                case "SetSingle":
                    Assert.Equal("Hi!", sth.Greeting);
                    break;
                case "SetMany":
                    Assert.Equal(new[] { "Liz", "Bob" }, sth.Names);
                    break;
            }
        }

        [Fact]
        public async Task Can_deserialize_single_expression()
        {
            var jsonFile = await Flexo.ReadFileAsync(@"ExpressionObject.json", MimeType.Json);
            using (var jsonStream = await jsonFile.CopyToMemoryStreamAsync())
            {
                var expression = await _helper.Serializer.DeserializeExpressionAsync(jsonStream.Rewind());
                Assert.NotNull(expression);
            }
        }

        private static IEnumerable<object> GetData() => new (string UseCaseName, object Expected, bool Throws)[]
        {
            ("Any", true, false),
            ("Any.ToDouble", 1.0, false),
            ("Any{Predicate=false}", true, false),
            ("All{Predicate=true}", false, false),
            ("All.ToDouble", 0.0, false),
            ("Sum", 3.0, false),
            ("ToDouble", 1.0, false),
            ("Matches", true, false),
            ("True.Not", false, false),
            ("True.ToDouble", 1.0, false),
            ("True.All-overrides-this", false, false),
            ("Double.ToDouble", 1.0, true),
            ("Double.IsEqual", true, false),
            ("Double.ref_IsLessThan3", true, false),
            ("CollectionOfDouble", new double[] { 1, 2, 3 }, false),
            ("CollectionOfDouble-2", new double[] { 1, 2, 3 }, false),
            ("Collection.Sum", 3.0, false),
            ("Collection.Max", 4.0, false),
            ("Collection.Contains{Comparer=Regex}", true, false),
            ("Collection.Contains{Comparer=Regex}", true, false),
            ("Collection.Any{Predicate=Matches}", true, false),
            ("Collection.Contains", true, false),
            ("Collection.All", true, false),
            ("Collection.Overlaps{Comparer=SoftString}", true, false),
            ("Collection.Select.ToString", new[] { "1", "True" }, false),
            ("String.In", true, false),
            ("Collection.Where.IsGreaterThan", new[] { 3.0, 4.0 }, false),
            ("GetSingle", "Hallo!", false),
            ("GetMany", new[] { "Joe", "Bob" }, false),
            ("GetMany.Contains", true, false),
            ("Concat", new[] { "Joe", "Bob", "Tom" }, false),
            ("Union", new[] { "Joe", "Bob", "Tom" }, false),
            ("GetMany.Block.Contains", true, false),
            ("String.IsNullOrEmpty-false", false, false),
            ("String.IsNullOrEmpty-true", true, false),
            ("SetSingle", null, false),
            ("SetMany", null, false),
        }.Select(uc => new { uc.UseCaseName, uc.Expected, uc.Throws });


        //[TypeNameFactory]
        [UseMember]
        [PlainSelectorFormatter]
        [TrimStart("I"), TrimEnd("Meta")]
        private interface ITestMeta
        {
            [UseMember]
            Something Sth { get; }
        }
        
        private class Something
        {
            public string Greeting { get; set; } = "Hallo!";

            public IEnumerable<string> Names { get; set; } = new[] { "Joe", "Bob" };

            public IEnumerable<Car> Cars { get; set; } = new[] { new Car { Make = "Audi" }, new Car { Make = "VW" } };
        }

        private class Car
        {
            public string Make { get; set; }
        }
    }
}