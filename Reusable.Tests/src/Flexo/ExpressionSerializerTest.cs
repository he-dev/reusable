using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Custom;
using System.Net.PeerToPeer;
using System.Threading.Tasks;
using Autofac;
using Reusable.Data;
using Reusable.Extensions;
using Reusable.Flexo;
using Reusable.Flexo.Helpers;
using Reusable.OmniLog;
using Reusable.Quickey;
using Reusable.Translucent;
using Reusable.Utilities.JsonNet;
using Reusable.Utilities.JsonNet.DependencyInjection;
using Reusable.Utilities.XUnit.Annotations;
using Xunit;
using Xunit.Abstractions;
using Double = Reusable.Flexo.Double;
using ExpressionSerializer = Reusable.Flexo.ExpressionSerializer;

// ReSharper disable once CheckNamespace
namespace Reusable.Flexo
{
    public class ExpressionSerializerTest : IClassFixture<ExpressionFixture>
    {
        private readonly ExpressionFixture _helper;
        private readonly ITestOutputHelper _output;

        public ExpressionSerializerTest(ExpressionFixture helper, ITestOutputHelper output)
        {
            _helper = helper;
            _output = output;
        }

        [Theory]
        [SmartMemberData(nameof(GetData))]
        public void Can_evaluate_supported_expressions(string useCaseName, object expected, bool throws, ISet<SoftString> tags)
        {
            var useCase = _helper.Expressions.Where(e => e.Name == useCaseName).SingleOrThrow();
            var sth = new Something();
            ExpressionAssert.ExpressionEqual
            (
                expected,
                useCase,
                ctx => ctx
                    .WithReferences(_helper.References)
                    .SetItem(From<ITestMeta>.Select(x => x.Sth), sth),
                _output,
                throws,
                tags
            );

//            switch (useCaseName)
//            {
//                case "Concat":
//                case "Union":
//                    Assert.Equal(new[] { "Joe", "Bob", "Tom" }, sth.Names);
//                    break;
//                case "SetSingle":
//                    Assert.Equal("Hi!", sth.Greeting);
//                    break;
//                case "SetMany":
//                    Assert.Equal(new[] { "Liz", "Bob" }, sth.Names);
//                    break;
//            }
        }

        [Fact]
        public async Task Can_deserialize_single_expression()
        {
            using (var jsonFile = await TestHelper.Resources.GetFileAsync(@"ExpressionObject.json", MimeType.Json))
            {
                var expression = await _helper.Serializer.DeserializeExpressionAsync(jsonFile.Body.Rewind());
                Assert.NotNull(expression);
            }
        }

        private static IEnumerable<object> GetData() => new (string UseCaseName, object Expected, bool Throws, ISet<SoftString> Tags)[]
        {
            ("Any", true, false, default),
            ("Any.ToDouble", 1.0, false, default),
            ("Any{Predicate=false}", true, false, default),
            ("All{Predicate=true}", false, false, default),
            ("All.ToDouble", 0.0, false, default),
            ("Sum", 3.0, false, default),
            ("ToDouble", 1.0, false, default),
            ("Matches", true, false, default),
            ("True.Not", false, false, default),
            ("True.ToDouble", 1.0, false, default),
            ("True.All-overrides-this", false, false, default),
            ("Double.ToDouble", 1.0, true, default),
            ("Double.IsEqual", true, false, default),
            ("Double.ref_IsLessThan3", true, false, default),
            ("CollectionOfDouble", new double[] { 1, 2, 3 }, false, default),
            ("CollectionOfDouble-2", new double[] { 1, 2, 3 }, false, default),
            ("Collection.Sum", 3.0, false, default),
            ("Collection.Max", 4.0, false, default),
            ("Collection.Contains{Comparer=Regex}", true, false, default),
            ("Collection.Contains{Comparer=Regex}", true, false, default),
            ("Collection.Any{Predicate=Matches}", true, false, default),
            ("Collection.Contains", true, false, default),
            ("Collection.All", true, false, default),
            ("Collection.Overlaps{Comparer=SoftString}", true, false, default),
            ("Collection.Select.ToString", new[] { "1", "True" }, false, default),
            ("String.In", true, false, default),
            ("Collection.Where.IsGreaterThan", new[] { 3.0, 4.0 }, false, default),
            ("GetSingle", "Hallo!", false, default),
            ("GetMany", new[] { "Joe", "Bob" }, false, default),
            ("GetMany.Contains", true, false, default),
            //("Concat", new[] { "Joe", "Bob", "Tom" }, false, default),
            //("Union", new[] { "Joe", "Bob", "Tom" }, false, default),
            ("GetMany.Block.Contains", true, false, default),
            ("String.IsNullOrEmpty-false", false, false, default),
            ("String.IsNullOrEmpty-true", true, false, default),
            //("SetSingle", null, false, default),
            //("SetMany", null, false, default),
            ("DoubleWithTags", 3.0, false, new HashSet<SoftString> { "tag-1,", "tag-2" }),
        }.Select(uc => new { uc.UseCaseName, uc.Expected, uc.Throws, uc.Tags });

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